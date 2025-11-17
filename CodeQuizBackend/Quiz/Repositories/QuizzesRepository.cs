using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeQuizBackend.Quiz.Repositories
{
    public class QuizzesRepository(ApplicationDbContext dbContext, IAppLogger<QuizzesRepository> logger) : IQuizzesRepository
    {
        public async Task<Models.Quiz> CreateQuizAsync(Models.Quiz quiz)
        {
            logger.LogInfo("Creating Quiz...");
            try
            {
                await dbContext.Quizzes.AddAsync(quiz);
                await dbContext.SaveChangesAsync();
                return quiz;
            }
            catch (Exception e)
            {
                logger.LogError($"Error creating quiz {e.Message}");
                throw;
            }
        }

        public async Task DeleteQuizAsync(int id)
        {
            var quiz = await dbContext.Quizzes.FindAsync(id) ?? throw new ResourceNotFoundException("Quiz not found");
            dbContext.Quizzes.Remove(quiz);
            await dbContext.SaveChangesAsync();
        }

        public async Task<Models.Quiz> GetQuizByIdAsync(int id)
        {
            var quiz = await dbContext.Quizzes.FindAsync(id) ?? throw new ResourceNotFoundException("Quiz not found");
            return quiz;
        }

        public async Task<List<Models.Quiz>> GetUserQuizzesAsync(string userId)
        {
            return await dbContext.Quizzes
                .Where(q => q.ExaminerId == userId)
                .Include(q => q.Questions)
                .ToListAsync();
        }

        public async Task<Models.Quiz> UpdateQuizAsync(Models.Quiz quiz)
        {
            var q = await dbContext.Quizzes.Include(q => q.Questions).Where(q => q.Id == quiz.Id).FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Quiz not found");
            q.Title = quiz.Title;
            q.StartDate = quiz.StartDate;
            q.EndDate = quiz.EndDate;
            q.Duration = quiz.Duration;
            q.Code = quiz.Code;
            q.GlobalQuestionConfiguration = quiz.GlobalQuestionConfiguration;
            q.Questions = quiz.Questions;
            await dbContext.SaveChangesAsync();
            return q;
        }

        public async Task<Models.Quiz> GetQuizByCodeAsync(string code)
        {
            var quiz = await dbContext.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Examiner)
                .FirstOrDefaultAsync(q => q.Code == code && q.EndDate > DateTime.Now) ?? throw new ResourceNotFoundException("Quiz not found");
            return quiz;
        }
    }
}
