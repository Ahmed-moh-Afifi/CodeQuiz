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

            var now = DateTime.Now;

            var expiredAttempts = await dbContext.Attempts
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(q => q.Solutions)
                .Where(a => a.EndTime == null && 
                           (now - a.StartTime > a.Quiz.Duration
                            || a.Quiz.EndDate < now))
                .ToListAsync();

            if (expiredAttempts.Any())
            {
                logger.LogInfo($"Found {expiredAttempts.Count} expired attempts to auto-submit.");

                foreach (var attempt in expiredAttempts)
                {
                    var durationEndTime = attempt.StartTime.Add(attempt.Quiz.Duration);
                    var quizEndTime = attempt.Quiz.EndDate;
                    
                    attempt.EndTime = durationEndTime < quizEndTime ? durationEndTime : quizEndTime;
                    await attemptsHubContext.Clients.All.SendAsync("AttemptAutoSubmitted", attempt.ToExamineeAttempt());
                    await attemptsService.EvaluateAttempt(attempt.Id);

                    logger.LogInfo(
                        $"Auto-submitted attempt {attempt.Id} for quiz {attempt.QuizId}. " +
                        $"EndTime set to {attempt.EndTime:yyyy-MM-dd HH:mm:ss}");
                }

                await dbContext.SaveChangesAsync();
                logger.LogInfo($"Successfully auto-submitted {expiredAttempts.Count} attempts.");
            }
        }
    }
}