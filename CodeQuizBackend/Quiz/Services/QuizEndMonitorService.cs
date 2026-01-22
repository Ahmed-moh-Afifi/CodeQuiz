using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Quiz.Models;
using CodeQuizBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// Background service that monitors quiz end times and sends summary emails to examiners
    /// when quizzes become unavailable.
    /// </summary>
    public class QuizEndMonitorService(
        IServiceProvider serviceProvider,
        IAppLogger<QuizEndMonitorService> logger) : BackgroundService
    {
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Small buffer after end time to ensure all auto-submissions are complete
        /// before sending the summary email.
        /// </summary>
        private readonly TimeSpan _emailDelayBuffer = TimeSpan.FromMinutes(2);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInfo("Quiz End Monitor Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckEndedQuizzesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError("Error occurred while checking ended quizzes.", ex);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            logger.LogInfo("Quiz End Monitor Service is stopping.");
        }

        private async Task CheckEndedQuizzesAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();

            // Calculate the cutoff time (now - buffer) so we compare EndDate directly
            // This avoids using DateTime.Add() in the LINQ query which EF can't translate
            var cutoffTime = DateTime.UtcNow - _emailDelayBuffer;

            // Find quizzes that:
            // 1. Have ended (EndDate <= cutoffTime)
            // 2. Haven't had their summary email sent yet (EndSummaryEmailSent == false)
            var endedQuizzes = await dbContext.Quizzes
                .Include(q => q.Examiner)
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                    .ThenInclude(a => a.Solutions)
                        .ThenInclude(s => s.AiAssessment)
                .Where(q => !q.EndSummaryEmailSent && q.EndDate <= cutoffTime)
                .ToListAsync(stoppingToken);

            foreach (var quiz in endedQuizzes)
            {
                try
                {
                    await SendQuizEndSummaryEmailAsync(quiz, mailService);

                    // Mark the quiz as having sent its summary email
                    quiz.EndSummaryEmailSent = true;
                    await dbContext.SaveChangesAsync(stoppingToken);

                    logger.LogInfo($"Sent quiz end summary email for quiz {quiz.Id} ({quiz.Title}) to {quiz.Examiner.Email}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to send quiz end summary email for quiz {quiz.Id}", ex);
                }
            }
        }

        private async Task SendQuizEndSummaryEmailAsync(Models.Quiz quiz, IMailService mailService)
        {
            var attempts = quiz.Attempts;
            var submittedAttempts = attempts.Where(a => a.EndTime != null).ToList();

            // Calculate statistics
            var totalAttempts = attempts.Count;
            var submittedCount = submittedAttempts.Count;
            var inProgressCount = attempts.Count(a => a.EndTime == null);

            var totalPoints = quiz.Questions.Sum(q => q.Points);
            var passThreshold = totalPoints * 0.6f; // 60% pass rate

            float averageGrade = 0;
            float highestGrade = 0;
            float lowestGrade = 0;
            float passRate = 0;

            if (submittedAttempts.Any())
            {
                var grades = submittedAttempts.Select(a => a.Solutions.Sum(s => s.ReceivedGrade ?? 0)).ToList();
                averageGrade = grades.Average();
                highestGrade = grades.Max();
                lowestGrade = grades.Min();
                passRate = (float)grades.Count(g => g >= passThreshold) / grades.Count * 100;
            }

            // AI assessment statistics
            var allSolutions = submittedAttempts.SelectMany(a => a.Solutions).ToList();
            var totalAiAssessments = allSolutions.Count(s => s.AiAssessment != null);
            var flaggedSolutions = allSolutions.Count(s => s.AiAssessment?.IsValid == false ||
                                                           (s.AiAssessment?.Flags?.Any() == true));

            var stats = new QuizEndStatistics
            {
                QuizTitle = quiz.Title,
                StartDate = quiz.StartDate,
                EndDate = quiz.EndDate,
                TotalAttempts = totalAttempts,
                SubmittedAttempts = submittedCount,
                InProgressAttempts = inProgressCount,
                AverageGrade = averageGrade,
                HighestGrade = highestGrade,
                LowestGrade = lowestGrade,
                PassRate = passRate,
                TotalAiAssessments = totalAiAssessments,
                FlaggedSolutions = flaggedSolutions,
                TotalPoints = totalPoints
            };

            await mailService.SendQuizEndSummaryAsync(
                quiz.Examiner.Email!,
                quiz.Examiner.FirstName,
                stats
            );
        }
    }
}
