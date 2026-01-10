using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Quiz.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CodeQuizBackend.Quiz.Services
{
    public class AttemptTimerService(IServiceProvider serviceProvider, IAppLogger<AttemptTimerService> logger, IHubContext<AttemptsHub> attemptsHubContext) : BackgroundService
    {
        private readonly TimeSpan checkInterval = TimeSpan.FromSeconds(10);
        
        /// <summary>
        /// Grace buffer period: The background service waits this amount of time after an attempt expires
        /// before auto-submitting. This allows late network packets from the frontend to arrive.
        /// </summary>
        private readonly TimeSpan graceBufferPeriod = TimeSpan.FromSeconds(30);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInfo("Attempt Timer Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await AutoSubmitExpiredAttemptsAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError("Error occurred while auto-submitting expired attempts.", ex);
                }

                await Task.Delay(checkInterval, stoppingToken);
            }

            logger.LogInfo("Attempt Timer Service is stopping.");
        }

        private async Task AutoSubmitExpiredAttemptsAsync()
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var attemptsService = scope.ServiceProvider.GetRequiredService<IAttemptsService>();

            // Use UTC time consistently for all comparisons
            var now = DateTime.UtcNow;

            // First, load all active attempts with their quiz data
            var activeAttempts = await dbContext.Attempts
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(q => q.Solutions)
                .Where(a => a.EndTime == null)
                .ToListAsync();

            // Filter expired attempts in memory to ensure consistent DateTime comparison
            // This avoids issues with EF Core translating DateTime arithmetic differently
            // Only target attempts that have been expired for at least the grace buffer period
            var expiredAttempts = activeAttempts
                .Where(a =>
                {
                    var durationEndTime = a.StartTime.Add(a.Quiz.Duration);
                    var quizEndTime = a.Quiz.EndDate;
                    var maxEndTime = durationEndTime < quizEndTime ? durationEndTime : quizEndTime;
                    
                    // Only mark as expired if the max end time plus grace buffer has passed
                    // This gives frontend clients time to submit their final saves
                    return maxEndTime.Add(graceBufferPeriod) <= now;
                })
                .ToList();

            if (expiredAttempts.Any())
            {
                logger.LogInfo($"Found {expiredAttempts.Count} expired attempts to auto-submit (past {graceBufferPeriod.TotalSeconds}s grace buffer). Current UTC time: {now:yyyy-MM-dd HH:mm:ss}");

                foreach (var attempt in expiredAttempts)
                {
                    var durationEndTime = attempt.StartTime.Add(attempt.Quiz.Duration);
                    var quizEndTime = attempt.Quiz.EndDate;
                    
                    attempt.EndTime = durationEndTime < quizEndTime ? durationEndTime : quizEndTime;
                    
                    logger.LogInfo(
                        $"Auto-submitting attempt {attempt.Id} for quiz {attempt.QuizId}. " +
                        $"StartTime: {attempt.StartTime:yyyy-MM-dd HH:mm:ss}, " +
                        $"Duration: {attempt.Quiz.Duration}, " +
                        $"QuizEndDate: {quizEndTime:yyyy-MM-dd HH:mm:ss}, " +
                        $"EndTime set to: {attempt.EndTime:yyyy-MM-dd HH:mm:ss}");
                    
                    var examineeAttempt = attempt.ToExamineeAttempt();
                    
                    // Send to specific groups instead of all clients
                    // Notify the examinee who owns the attempt
                    await attemptsHubContext.Clients.Group($"user_{attempt.ExamineeId}")
                        .SendAsync("AttemptAutoSubmitted", examineeAttempt);
                        
                    // Notify the examiner who owns the quiz
                    await attemptsHubContext.Clients.Group($"examiner_{attempt.Quiz.ExaminerId}")
                        .SendAsync("AttemptAutoSubmitted", examineeAttempt);
                    
                    await attemptsService.EvaluateAttempt(attempt.Id);
                }

                await dbContext.SaveChangesAsync();
                logger.LogInfo($"Successfully auto-submitted {expiredAttempts.Count} attempts.");
            }
        }
    }
}