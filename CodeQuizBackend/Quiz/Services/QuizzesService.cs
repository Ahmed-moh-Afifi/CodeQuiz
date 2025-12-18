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
    public class QuizzesService(IQuizzesRepository quizzesRepository, ApplicationDbContext dbContext, QuizCodeGenerator quizCodeGenerator, IHubContext<QuizzesHub> quizzesHubContext, IAppLogger<QuizzesService> logger) : IQuizzesService
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
            await quizzesRepository.DeleteQuizAsync(id);
            await quizzesHubContext.Clients.All.SendAsync("QuizDeleted", id);
        }

        public async Task<ExamineeQuiz> GetQuizByCode(string code)
        {
            var quiz = await quizzesRepository.GetQuizByCodeAsync(code);
            return quiz.ToExamineeQuiz();
        }

        public async Task<List<ExaminerQuiz>> GetUserQuizzes(string userId)
        {
            var quizzesStatistics  = await dbContext.Quizzes
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

        public async Task<ExaminerQuiz> UpdateQuiz(ExaminerQuiz quiz)
        {
            var quizEntity = await dbContext.Quizzes
                .Where(q => q.Id == quiz.Id)
                .Include(q => q.Attempts)
                .ThenInclude(a => a.Solutions)
                .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Quiz not found. It may have been deleted.");
            if (quizEntity.EndDate <= DateTime.Now && quiz.EndDate > DateTime.Now)
            {
                quiz.Code = await quizCodeGenerator.GenerateUniqueQuizCode();
            }

            quizEntity = new Models.Quiz
            {
                Id = quiz.Id,
                Title = quiz.Title,
                StartDate = quiz.StartDate,
                EndDate = quiz.EndDate,
                Duration = quiz.Duration,
                Code = quiz.Code,
                ExaminerId = quiz.ExaminerId,
                GlobalQuestionConfiguration = quiz.GlobalQuestionConfiguration,
                AllowMultipleAttempts = quiz.AllowMultipleAttempts,
                TotalPoints = quiz.TotalPoints,
                Questions = quiz.Questions.Select(q => new Question
                {
                    Id = q.Id,
                    Statement = q.Statement,
                    EditorCode = q.EditorCode,
                    QuestionConfiguration = q.QuestionConfiguration,
                    TestCases = q.TestCases,
                    QuizId = q.QuizId,
                    Order = q.Order,
                    Points = q.Points
                }).ToList()
            };

            var updatedQuiz = await quizzesRepository.UpdateQuizAsync(quizEntity);
            var statistics = await dbContext.Quizzes.Where(q => q.Id == updatedQuiz.Id).Select(q => new 
            {
                AttemptsCount = q.Attempts.Count,
                SubmittedAttemptsCount = q.Attempts.Count(a => a.EndTime != null),
                AverageAttemptScore = q.Attempts.Where(a => a.Solutions.All(s => s.ReceivedGrade != null)).Select(a => a.Solutions.Sum(s => s.ReceivedGrade)).DefaultIfEmpty().Average() ?? 0
            }).FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Quiz not found."); // This approach is going be changed after the demo :)
            var updatedExaminerQuiz = updatedQuiz.ToExaminerQuiz(statistics.AttemptsCount, statistics.SubmittedAttemptsCount, statistics.AverageAttemptScore);
            var updatedExamineeQuiz = updatedQuiz.ToExamineeQuiz();
            await quizzesHubContext.Clients.All.SendAsync("QuizUpdated", updatedExaminerQuiz, updatedExamineeQuiz);
            return updatedExaminerQuiz;
        }

        public async Task<List<ExaminerAttempt>> GetQuizAttempts(int quizId)
        {
            var examinerAttempts = await dbContext.Attempts
                .Where(a => a.QuizId == quizId)
                .Include(a => a.Solutions)
                .Include(a => a.Examinee)
                .Select(a => a.ToExaminerAttempt())
                .ToListAsync();
            return examinerAttempts;
        }
    }
}
