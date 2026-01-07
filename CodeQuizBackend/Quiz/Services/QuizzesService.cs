using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Models;
using CodeQuizBackend.Quiz.Models.DTOs;
using CodeQuizBackend.Quiz.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CodeQuizBackend.Quiz.Services
{
    public class QuizzesService(IQuizzesRepository quizzesRepository, ApplicationDbContext dbContext, IQuizCodeGenerator quizCodeGenerator, IHubContext<QuizzesHub> quizzesHubContext, IAppLogger<QuizzesService> logger) : IQuizzesService
    {
        private static readonly SemaphoreSlim _quizCreationLock = new(1, 1);

        public async Task<ExaminerQuiz> CreateQuiz(NewQuizModel newQuizModel)
        {
            await _quizCreationLock.WaitAsync();
            try
            {
                logger.LogInfo("Creating Quiz...");
                var code = await quizCodeGenerator.GenerateUniqueQuizCode();

                var quiz = newQuizModel.ToQuiz(code);

                var createdQuiz = await quizzesRepository.CreateQuizAsync(quiz);

                return createdQuiz.ToExaminerQuiz(0, 0, 0);
            }
            finally
            {
                _quizCreationLock.Release();
            }
        }

        public async Task DeleteQuiz(int id)
        {
            // Get the quiz to find the examiner before deleting
            var quiz = await dbContext.Quizzes.FindAsync(id);
            var examinerId = quiz?.ExaminerId;
            
            await quizzesRepository.DeleteQuizAsync(id);
            
            // Only notify the examiner who owns the quiz
            if (examinerId != null)
            {
                await quizzesHubContext.Clients.Group($"examiner_{examinerId}")
                    .SendAsync("QuizDeleted", id);
            }
        }

        public async Task<ExamineeQuiz> GetQuizByCode(string code)
        {
            var quiz = await quizzesRepository.GetQuizByCodeAsync(code);
            return quiz.ToExamineeQuiz();
        }

        public async Task<List<ExaminerQuiz>> GetUserQuizzes(string userId)
        {
            var quizzesStatistics = await dbContext.Quizzes
                .Where(q => q.ExaminerId == userId)
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                .ThenInclude(a => a.Solutions)
                .Select(q => new
                {
                    Quiz = q,
                    AttemptsCount = q.Attempts.Count,
                    SubmittedAttemptsCount = q.Attempts.Count(a => a.EndTime != null),
                    AverageAttemptScore = q.Attempts.Where(a => a.Solutions.All(s => s.ReceivedGrade != null)).Select(a => a.Solutions.Sum(s => s.ReceivedGrade)).DefaultIfEmpty().Average() ?? 0
                })
                .ToListAsync();

            return quizzesStatistics.Select(qs => qs.Quiz.ToExaminerQuiz(qs.AttemptsCount, qs.SubmittedAttemptsCount, qs.AverageAttemptScore)).ToList();
        }

        public async Task<ExaminerQuiz> UpdateQuiz(int id, NewQuizModel newQuizModel)
        {
            var quizEntity = await dbContext.Quizzes
                .Where(q => q.Id == id)
                .Include(q => q.Questions)
                .Include(q => q.Examiner)
                .Include(q => q.Attempts)
                .ThenInclude(a => a.Solutions)
                .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Quiz not found. It may have been deleted.");

            var newCode = quizEntity.Code;
            // Use UTC time for quiz status check
            if (quizEntity.EndDate <= DateTime.UtcNow && newQuizModel.EndDate > DateTime.UtcNow)
            {
                newCode = await quizCodeGenerator.GenerateUniqueQuizCode();
            }

            var quizEntity2 = new Models.Quiz
            {
                Id = id,
                Title = newQuizModel.Title,
                StartDate = newQuizModel.StartDate,
                EndDate = newQuizModel.EndDate,
                Duration = newQuizModel.Duration,
                Code = newCode,
                ExaminerId = newQuizModel.ExaminerId,
                GlobalQuestionConfiguration = newQuizModel.GlobalQuestionConfiguration,
                AllowMultipleAttempts = newQuizModel.AllowMultipleAttempts,
                Questions = newQuizModel.Questions.Select(q => q.ToQuestion()).ToList()
            };

            var updatedQuiz = await quizzesRepository.UpdateQuizAsync(quizEntity2);
            var statistics = await dbContext.Quizzes.Where(q => q.Id == updatedQuiz.Id).Select(q => new
            {
                AttemptsCount = q.Attempts.Count,
                SubmittedAttemptsCount = q.Attempts.Count(a => a.EndTime != null),
                AverageAttemptScore = q.Attempts.Where(a => a.Solutions.All(s => s.ReceivedGrade != null)).Select(a => a.Solutions.Sum(s => s.ReceivedGrade)).DefaultIfEmpty().Average() ?? 0
            }).FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Quiz not found.");
            var updatedExaminerQuiz = updatedQuiz.ToExaminerQuiz(statistics.AttemptsCount, statistics.SubmittedAttemptsCount, statistics.AverageAttemptScore);
            var updatedExamineeQuiz = updatedQuiz.ToExamineeQuiz();
            
            // Only notify the examiner who owns the quiz
            await quizzesHubContext.Clients.Group($"examiner_{updatedQuiz.ExaminerId}")
                .SendAsync("QuizUpdated", updatedExaminerQuiz, updatedExamineeQuiz);
                
            return updatedExaminerQuiz;
        }

        public async Task<List<ExaminerAttempt>> GetQuizAttempts(int quizId)
        {
            var examinerAttempts = await dbContext.Attempts
                .Where(a => a.QuizId == quizId)
                .Include(a => a.Solutions)
                .Include(a => a.Examinee)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Select(a => a.ToExaminerAttempt())
                .ToListAsync();
            return examinerAttempts;
        }
    }
}
