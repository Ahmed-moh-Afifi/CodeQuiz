using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Quiz.Models;
using CodeQuizBackend.Quiz.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CodeQuizBackend.Quiz.Services
{
    public class AttemptsService(ApplicationDbContext dbContext) : IAttemptsService
    {
        public async Task<ExamineeAttempt> BeginAttempt(string quizCode, string examineeId)
        {
            var attempt = await dbContext.Attempts
                .Where(a => a.Quiz.Code == quizCode && a.ExamineeId == examineeId && a.EndTime == null)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(q => q.Solutions)
                .FirstOrDefaultAsync();

            if (attempt == null)
            {
                var hasExpiredAttempt = await dbContext.Attempts
                    .AnyAsync(a => a.Quiz.Code == quizCode && a.ExamineeId == examineeId && a.EndTime != null);

                var quiz = await dbContext.Quizzes
                .Where(q => q.Code == quizCode &&
                            DateTime.Now > q.StartDate &&
                            DateTime.Now < q.EndDate)
                .Include(q => q.Examiner)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync()
                ?? throw new ResourceNotFoundException("Quiz not found");

                if (hasExpiredAttempt && !quiz.AllowMultipleAttempts)
                {
                    throw new InvalidOperationException("Multiple attempts are not allowed for this quiz.");
                }

                attempt = new()
                {
                    Id = 0,
                    StartTime = DateTime.Now,
                    QuizId = quiz.Id,
                    ExamineeId = examineeId,
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
                .FirstOrDefaultAsync()
                ?? throw new ResourceNotFoundException("Attempt not found");

            attempt.EndTime ??= DateTime.Now;
            await dbContext.SaveChangesAsync();
            return attempt.ToExamineeAttempt();
        }

        public async Task<SolutionDTO> UpdateSolution(SolutionDTO solution)
        {
            var sol = await dbContext.Solutions.FindAsync(solution.Id)
                ?? throw new ResourceNotFoundException("Solution not found");

            sol.Code = solution.Code;
            await dbContext.SaveChangesAsync();
            return sol.ToDTO();
        }
    }
}
