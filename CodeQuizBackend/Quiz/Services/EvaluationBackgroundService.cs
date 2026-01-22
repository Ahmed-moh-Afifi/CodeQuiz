using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Execution.Models;
using CodeQuizBackend.Execution.Services;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Models;
using CodeQuizBackend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// Background service that processes evaluation jobs from the queue.
    /// Handles both system grading and AI assessment with real-time SignalR notifications.
    /// </summary>
    public class EvaluationBackgroundService : BackgroundService
    {
        private readonly IEvaluationQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppLogger<EvaluationBackgroundService> _logger;
        private readonly IHubContext<AttemptsHub> _attemptsHubContext;
        private readonly IHubContext<QuizzesHub> _quizzesHubContext;

        public EvaluationBackgroundService(
            IEvaluationQueue queue,
            IServiceProvider serviceProvider,
            IAppLogger<EvaluationBackgroundService> logger,
            IHubContext<AttemptsHub> attemptsHubContext,
            IHubContext<QuizzesHub> quizzesHubContext)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _attemptsHubContext = attemptsHubContext;
            _quizzesHubContext = quizzesHubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInfo("Evaluation Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var job = await _queue.DequeueAsync(stoppingToken);
                    await ProcessEvaluationJobAsync(job, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing evaluation job: {ex.Message}", ex);
                }
            }

            _logger.LogInfo("Evaluation Background Service is stopping.");
        }

        private async Task ProcessEvaluationJobAsync(EvaluationJob job, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var evaluator = scope.ServiceProvider.GetRequiredService<IEvaluator>();
            var aiAssessmentService = scope.ServiceProvider.GetRequiredService<IAiAssessmentService>();
            var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();

            _logger.LogInfo($"Processing evaluation job for attempt {job.AttemptId}");

            // Notify that evaluation has started
            await NotifyEvaluationStatusAsync(job, "EvaluationStarted", null);

            try
            {
                var attempt = await dbContext.Attempts
                    .Where(a => a.Id == job.AttemptId)
                    .Include(a => a.Examinee)
                    .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.TestCases)
                    .Include(a => a.Solutions)
                    .Include(a => a.Quiz)
                    .ThenInclude(q => q.Examiner)
                    .FirstOrDefaultAsync(stoppingToken);

                if (attempt == null)
                {
                    _logger.LogWarning($"Attempt {job.AttemptId} not found for evaluation");
                    await NotifyEvaluationStatusAsync(job, "EvaluationFailed", "Attempt not found");
                    return;
                }

                if (attempt.EndTime == null)
                {
                    _logger.LogWarning($"Attempt {job.AttemptId} has not been submitted yet");
                    await NotifyEvaluationStatusAsync(job, "EvaluationFailed", "Attempt not submitted");
                    return;
                }

                // Step 1: System Grading
                _logger.LogInfo($"Starting system grading for attempt {job.AttemptId}");
                var systemGradingResult = await PerformSystemGradingAsync(attempt, evaluator, stoppingToken);
                await dbContext.SaveChangesAsync(stoppingToken);

                // Notify system grading complete
                var examinerAttempt = attempt.ToExaminerAttempt();
                var examineeAttempt = attempt.ToExamineeAttempt();
                await NotifyEvaluationStatusAsync(job, "SystemGradingComplete", null, examinerAttempt, examineeAttempt);

                // Step 2: AI Assessment (for solutions that passed at least some test cases)
                _logger.LogInfo($"Starting AI assessment for attempt {job.AttemptId}");
                await PerformAiAssessmentAsync(attempt, aiAssessmentService, dbContext, stoppingToken);
                await dbContext.SaveChangesAsync(stoppingToken);

                // Final notification with AI assessment
                // Reload attempt to get AI assessments
                attempt = await dbContext.Attempts
                    .Where(a => a.Id == job.AttemptId)
                    .Include(a => a.Examinee)
                    .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                    .Include(a => a.Solutions)
                    .ThenInclude(s => s.AiAssessment)
                    .Include(a => a.Quiz)
                    .ThenInclude(q => q.Examiner)
                    .FirstAsync(stoppingToken);

                examinerAttempt = attempt.ToExaminerAttempt();
                examineeAttempt = attempt.ToExamineeAttempt();

                await NotifyEvaluationStatusAsync(job, "AiAssessmentComplete", null, examinerAttempt, examineeAttempt);

                // Send email notification
                if (systemGradingResult.AllGraded)
                {
                    await mailService.SendAttemptFeedbackAsync(
                        attempt.Examinee.Email!,
                        attempt.Examinee.FirstName,
                        attempt.Quiz.Title,
                        (float)examineeAttempt.Grade!,
                        attempt.Quiz.ToExamineeQuiz().TotalPoints,
                        attempt.StartTime,
                        DateTime.UtcNow);
                }

                // Notify quiz statistics updated
                await NotifyQuizStatisticsUpdatedAsync(job.QuizId, job.ExaminerId, dbContext);

                _logger.LogInfo($"Evaluation complete for attempt {job.AttemptId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Evaluation failed for attempt {job.AttemptId}: {ex.Message}", ex);
                await NotifyEvaluationStatusAsync(job, "EvaluationFailed", ex.Message);
            }
        }

        private async Task<SystemGradingResult> PerformSystemGradingAsync(
            Attempt attempt,
            IEvaluator evaluator,
            CancellationToken stoppingToken)
        {
            var gradedCount = 0;

            foreach (var solution in attempt.Solutions)
            {
                stoppingToken.ThrowIfCancellationRequested();

                var question = attempt.Quiz.Questions.First(q => q.Id == solution.QuestionId);
                if (!question.TestCases.IsNullOrEmpty())
                {
                    var questionConfig = question.QuestionConfiguration ?? attempt.Quiz.GlobalQuestionConfiguration;
                    var evaluationResults = new List<EvaluationResult>();

                    foreach (var testCase in question.TestCases)
                    {
                        var result = await evaluator.EvaluateAsync(questionConfig.Language, solution.Code, testCase);
                        evaluationResults.Add(result);
                    }

                    solution.EvaluationResults = evaluationResults;

                    // Percentage-based grading
                    var passedCount = evaluationResults.Count(r => r.IsSuccessful);
                    var totalCount = evaluationResults.Count;
                    var passRatio = totalCount > 0 ? (float)passedCount / totalCount : 0;
                    solution.ReceivedGrade = passRatio * question.Points;
                    solution.EvaluatedBy = "System";
                    gradedCount++;
                }
            }

            return new SystemGradingResult { AllGraded = gradedCount == attempt.Solutions.Count };
        }

        private async Task PerformAiAssessmentAsync(
            Attempt attempt,
            IAiAssessmentService aiAssessmentService,
            ApplicationDbContext dbContext,
            CancellationToken stoppingToken)
        {
            foreach (var solution in attempt.Solutions)
            {
                stoppingToken.ThrowIfCancellationRequested();

                // Only assess solutions that have evaluation results
                if (solution.EvaluationResults == null || solution.EvaluationResults.Count == 0)
                    continue;

                var question = attempt.Quiz.Questions.First(q => q.Id == solution.QuestionId);
                var questionConfig = question.QuestionConfiguration ?? attempt.Quiz.GlobalQuestionConfiguration;

                try
                {
                    var assessment = await aiAssessmentService.AssessSolutionAsync(
                        solution, question, questionConfig, stoppingToken);

                    assessment.SolutionId = solution.Id;
                    dbContext.Set<AiAssessment>().Add(assessment);
                    solution.AiAssessment = assessment;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"AI assessment failed for solution {solution.Id}: {ex.Message}");
                    // Continue with other solutions even if one fails
                }
            }
        }

        private async Task NotifyEvaluationStatusAsync(
            EvaluationJob job,
            string eventName,
            string? errorMessage,
            object? examinerAttempt = null,
            object? examineeAttempt = null)
        {
            var payload = new
            {
                AttemptId = job.AttemptId,
                QuizId = job.QuizId,
                Status = eventName,
                ErrorMessage = errorMessage,
                ExaminerAttempt = examinerAttempt,
                ExamineeAttempt = examineeAttempt,
                Timestamp = DateTime.UtcNow
            };

            // Notify the examinee
            await _attemptsHubContext.Clients.Group($"user_{job.ExamineeId}")
                .SendAsync(eventName, payload);

            // Notify the examiner
            await _attemptsHubContext.Clients.Group($"examiner_{job.ExaminerId}")
                .SendAsync(eventName, payload);
        }

        private async Task NotifyQuizStatisticsUpdatedAsync(int quizId, string examinerId, ApplicationDbContext dbContext)
        {
            var quizWithStats = await dbContext.Quizzes
                .Where(q => q.Id == quizId)
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                .ThenInclude(a => a.Solutions)
                .Select(q => new
                {
                    Quiz = q,
                    AttemptsCount = q.Attempts.Count,
                    SubmittedAttemptsCount = q.Attempts.Count(a => a.EndTime != null),
                    AverageAttemptScore = q.Attempts
                        .Where(a => a.Solutions.All(s => s.ReceivedGrade != null))
                        .Select(a => a.Solutions.Sum(s => s.ReceivedGrade))
                        .DefaultIfEmpty()
                        .Average() ?? 0
                })
                .FirstOrDefaultAsync();

            if (quizWithStats == null) return;

            var examinerQuiz = quizWithStats.Quiz.ToExaminerQuiz(
                quizWithStats.AttemptsCount,
                quizWithStats.SubmittedAttemptsCount,
                quizWithStats.AverageAttemptScore);

            await _quizzesHubContext.Clients.Group($"examiner_{examinerId}")
                .SendAsync("QuizUpdated", examinerQuiz);
        }

        private class SystemGradingResult
        {
            public bool AllGraded { get; set; }
        }
    }
}
