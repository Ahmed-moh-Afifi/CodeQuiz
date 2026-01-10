using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Execution.Services;
using CodeQuizBackend.Quiz.Exceptions;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Models;
using CodeQuizBackend.Quiz.Models.DTOs;
using CodeQuizBackend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CodeQuizBackend.Quiz.Services
{
    public class AttemptsService(ApplicationDbContext dbContext, IEvaluator evaluator, IHubContext<AttemptsHub> attemptsHubContext, IHubContext<QuizzesHub> quizzesHubContext, IMailService mailService) : IAttemptsService
    {
        /// <summary>
        /// Maximum network allowance in seconds after attempt deadline for accepting saves.
        /// This prevents exploitation of the grace buffer while allowing for network latency.
        /// </summary>
        private const int NetworkAllowanceSeconds = 5;

        public async Task<ExamineeAttempt> BeginAttempt(string quizCode, string examineeId)
        {
            var attempt = await dbContext.Attempts
                .Where(a => a.Quiz.Code == quizCode && a.ExamineeId == examineeId && a.EndTime == null)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(a => a.Solutions)
                .Include(a => a.Examinee)
                .FirstOrDefaultAsync();

            if (attempt == null)
            {
                var hasExpiredAttempt = await dbContext.Attempts
                    .AnyAsync(a => a.Quiz.Code == quizCode && a.ExamineeId == examineeId && a.EndTime != null);

                var quiz = await dbContext.Quizzes
                .Where(q => q.Code == quizCode)
                .Include(q => q.Examiner)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();

                if (quiz == null)
                {
                    throw new ResourceNotFoundException("Quiz not found. Please check the code and try again.");
                }

                // Use UTC time for all quiz availability checks
                var now = DateTime.UtcNow;
                
                if (now < quiz.StartDate)
                {
                    throw new QuizNotActiveException("This quiz has not started yet. Please wait until the scheduled start time.");
                }

                if (now > quiz.EndDate)
                {
                    throw new QuizNotActiveException("This quiz has ended and is no longer accepting new attempts.");
                }

                if (hasExpiredAttempt && !quiz.AllowMultipleAttempts)
                {
                    throw new MultipleAttemptsNotAllowedException();
                }

                var examinee = await dbContext.Users.FindAsync(examineeId)
                    ?? throw new ResourceNotFoundException("Examinee not found");

                attempt = new()
                {
                    Id = 0,
                    StartTime = DateTime.UtcNow, // Use UTC for consistent time tracking
                    QuizId = quiz.Id,
                    ExamineeId = examineeId,
                    Examinee = examinee,
                    Quiz = quiz,
                    Solutions = quiz.Questions.Select(q => new Solution
                    {
                        Id = 0,
                        AttemptId = 0,
                        QuestionId = q.Id,
                        Code = q.EditorCode,
                    }).ToList()
                };
                dbContext.Attempts.Add(attempt);
                await dbContext.SaveChangesAsync();
                
                // Send to specific groups instead of all clients
                var examinerAttempt = attempt.ToExaminerAttempt();
                var examineeAttempt = attempt.ToExamineeAttempt();
                
                // Notify the examinee (user who started the attempt)
                await attemptsHubContext.Clients.Group($"user_{examineeId}")
                    .SendAsync("AttemptCreated", examinerAttempt, examineeAttempt);
                    
                // Notify the examiner (quiz creator)
                await attemptsHubContext.Clients.Group($"examiner_{quiz.ExaminerId}")
                    .SendAsync("AttemptCreated", examinerAttempt, examineeAttempt);
                
                // Send updated quiz statistics to the examiner via QuizzesHub
                await NotifyQuizStatisticsUpdatedAsync(quiz.Id, quiz.ExaminerId);
            }

            return attempt.ToExamineeAttempt();
        }

        public async Task<List<ExamineeAttempt>> GetExamineeAttempts(string examineeId)
        {
            var examineeAttempts = await dbContext.Attempts
                .Where(a => a.ExamineeId == examineeId)
                .Include(a => a.Solutions)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Select(a => a.ToExamineeAttempt())
                .ToListAsync();
            return examineeAttempts;
        }

        public async Task<ExamineeAttempt> SubmitAttempt(int attemptId)
        {
            var attempt = await dbContext.Attempts
                .Where(a => a.Id == attemptId)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(a => a.Solutions)
                .Include(a => a.Examinee)
                .FirstOrDefaultAsync()
                ?? throw new ResourceNotFoundException("Attempt not found.");

            attempt.EndTime ??= DateTime.UtcNow; // Use UTC for consistent time tracking
            await dbContext.SaveChangesAsync();
            
            // Send to specific groups instead of all clients
            var examinerAttempt = attempt.ToExaminerAttempt();
            var examineeAttempt = attempt.ToExamineeAttempt();
            
            // Notify the examinee
            await attemptsHubContext.Clients.Group($"user_{attempt.ExamineeId}")
                .SendAsync("AttemptUpdated", examinerAttempt, examineeAttempt);
                
            // Notify the examiner
            await attemptsHubContext.Clients.Group($"examiner_{attempt.Quiz.ExaminerId}")
                .SendAsync("AttemptUpdated", examinerAttempt, examineeAttempt);
            
            // Send updated quiz statistics to the examiner via QuizzesHub
            await NotifyQuizStatisticsUpdatedAsync(attempt.QuizId, attempt.Quiz.ExaminerId);
                
            await EvaluateAttempt(attempt.Id);
            return examineeAttempt;
        }

        public async Task<SolutionDTO> UpdateSolution(SolutionDTO solution)
        {
            var sol = await dbContext.Solutions
                .Include(s => s.Attempt)
                .ThenInclude(a => a.Quiz)
                .FirstOrDefaultAsync(s => s.Id == solution.Id)
                ?? throw new ResourceNotFoundException("Solution not found.");

            // Enforce strict deadline: Reject saves after attempt deadline + network allowance
            var attempt = sol.Attempt;
            var now = DateTime.UtcNow;
            
            // Calculate the actual deadline (min of duration-based end time and quiz end date)
            var durationEndTime = attempt.StartTime.Add(attempt.Quiz.Duration);
            var quizEndTime = attempt.Quiz.EndDate;
            var attemptDeadline = durationEndTime < quizEndTime ? durationEndTime : quizEndTime;
            
            // Add network allowance to the deadline
            var strictDeadline = attemptDeadline.AddSeconds(NetworkAllowanceSeconds);
            
            // If the attempt is already submitted (has EndTime), reject the save
            if (attempt.EndTime != null)
            {
                throw new DeadlineExceededException("This attempt has already been submitted. Your solution cannot be saved.");
            }
            
            // If we're past the strict deadline, reject the save
            if (now > strictDeadline)
            {
                throw new DeadlineExceededException($"The deadline for this attempt has passed. Solutions must be saved within {NetworkAllowanceSeconds} seconds of the time limit.");
            }

            // Only update the code - grading is handled by UpdateSolutionGrade
            sol.Code = solution.Code;
            await dbContext.SaveChangesAsync();
            return sol.ToDTO();
        }

        /// <summary>
        /// Updates only the grade and evaluator for a solution.
        /// Used by instructors for manual grading, separate from student code saves.
        /// No deadline restrictions apply - instructors can grade at any time.
        /// </summary>
        public async Task<SolutionDTO> UpdateSolutionGrade(UpdateSolutionGradeRequest request)
        {
            var sol = await dbContext.Solutions
                .Include(s => s.Attempt)
                .ThenInclude(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(s => s.Attempt)
                .ThenInclude(a => a.Examinee)
                .FirstOrDefaultAsync(s => s.Id == request.SolutionId)
                ?? throw new ResourceNotFoundException("Solution not found.");

            // Update only grading-related fields
            sol.ReceivedGrade = request.ReceivedGrade;
            sol.EvaluatedBy = request.EvaluatedBy;
            
            await dbContext.SaveChangesAsync();
            
            // Notify both examinee and examiner about the grade update
            var attempt = sol.Attempt;
            
            // Reload attempt with all related data for DTOs
            var fullAttempt = await dbContext.Attempts
                .Where(a => a.Id == attempt.Id)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(a => a.Solutions)
                .Include(a => a.Examinee)
                .FirstAsync();
            
            var examinerAttempt = fullAttempt.ToExaminerAttempt();
            var examineeAttempt = fullAttempt.ToExamineeAttempt();
            
            // Notify the examinee about their grade
            await attemptsHubContext.Clients.Group($"user_{attempt.ExamineeId}")
                .SendAsync("AttemptUpdated", examinerAttempt, examineeAttempt);
                
            // Notify the examiner
            await attemptsHubContext.Clients.Group($"examiner_{attempt.Quiz.ExaminerId}")
                .SendAsync("AttemptUpdated", examinerAttempt, examineeAttempt);
            
            // Send updated quiz statistics (average score may have changed)
            await NotifyQuizStatisticsUpdatedAsync(attempt.QuizId, attempt.Quiz.ExaminerId);
            
            return sol.ToDTO();
        }

        public async Task EvaluateAttempt(int attemptId)
        {
            var attempt = await dbContext.Attempts
                .Where(a => a.Id == attemptId)
                .Include(a => a.Examinee)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(a => a.Solutions)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .FirstOrDefaultAsync()
                ?? throw new ResourceNotFoundException("Attempt not found.");

            if (attempt.EndTime == null)
            {
                throw new AttemptNotSubmittedException();
            }

            var evaluated = GradeAttemptSolutions(attempt);
            await dbContext.SaveChangesAsync();
            var examineeAttempt = attempt.ToExamineeAttempt();
            var examinerAttempt = attempt.ToExaminerAttempt();
            if (evaluated) await mailService.SendAttemptFeedbackAsync(attempt.Examinee.Email!, attempt.Examinee.FirstName, attempt.Quiz.Title, (float)examineeAttempt.Grade!, attempt.Quiz.ToExamineeQuiz().TotalPoints, attempt.StartTime, DateTime.UtcNow);
            
            // Send to specific groups instead of all clients
            // Notify the examinee
            await attemptsHubContext.Clients.Group($"user_{attempt.ExamineeId}")
                .SendAsync("AttemptUpdated", examinerAttempt, examineeAttempt);
                
            // Notify the examiner
            await attemptsHubContext.Clients.Group($"examiner_{attempt.Quiz.ExaminerId}")
                .SendAsync("AttemptUpdated", examinerAttempt, examineeAttempt);
            
            // Send updated quiz statistics (average score changed after evaluation)
            await NotifyQuizStatisticsUpdatedAsync(attempt.QuizId, attempt.Quiz.ExaminerId);
        }

        /// <summary>
        /// Notifies the examiner about updated quiz statistics via the QuizzesHub.
        /// Called when attempts are created, submitted, or graded.
        /// </summary>
        private async Task NotifyQuizStatisticsUpdatedAsync(int quizId, string examinerId)
        {
            // Fetch updated quiz with fresh statistics
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
            
            var examineeQuiz = quizWithStats.Quiz.ToExamineeQuiz();

            // Notify the examiner via QuizzesHub
            await quizzesHubContext.Clients.Group($"examiner_{examinerId}")
                .SendAsync("QuizUpdated", examinerQuiz, examineeQuiz);
        }

        private bool GradeAttemptSolutions(Attempt attempt)
        {
            var evaluatedCount = 0;
            foreach (var solution in attempt.Solutions)
            {
                var question = attempt.Quiz.Questions.First(q => q.Id == solution.QuestionId);
                if (!question.TestCases.IsNullOrEmpty())
                {
                    var correctSolution = TestCasesPassed(attempt.Quiz, question, solution);
                    solution.ReceivedGrade = correctSolution ? question.Points : 0;
                    solution.EvaluatedBy = "System";
                    evaluatedCount++;
                }
            }

            return evaluatedCount == attempt.Solutions.Count;
        }

        private bool TestCasesPassed(Quiz.Models.Quiz quiz, Question question, Solution solution)
        {
            var questionConfig = question.QuestionConfiguration ?? quiz.GlobalQuestionConfiguration;
            foreach (var testCase in question.TestCases)
            {
                var result = evaluator.EvaluateAsync(questionConfig.Language, solution.Code, testCase).Result;
                if (!result.IsSuccessful)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
