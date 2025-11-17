using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Quiz.Models.DTOs;
using CodeQuizBackend.Quiz.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeQuizBackend.Quiz.Services
{
    public class QuizzesService(IQuizzesRepository quizzesRepository, ApplicationDbContext dbContext, QuizCodeGenerator quizCodeGenerator, IAppLogger<QuizzesService> logger) : IQuizzesService
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

                return createdQuiz.ToExaminerQuiz();
            }
            catch (Exception e)
            {
                logger.LogError($"Error creating quiz {e.Message}");
                throw;
            }
            finally
            {
                _quizCreationLock.Release();
            }
        }

        public async Task DeleteQuiz(int id)
        {
            await quizzesRepository.DeleteQuizAsync(id);
        }

        public async Task<ExamineeQuiz> GetQuizByCode(string code)
        {
            var quiz = await quizzesRepository.GetQuizByCodeAsync(code);
            return quiz.ToExamineeQuiz();
        }

        public async Task<List<ExaminerQuiz>> GetUserQuizzes(string userId)
        {
            var quizzes = await quizzesRepository.GetUserQuizzesAsync(userId);
            return quizzes.Select(q => q.ToExaminerQuiz()).ToList();
        }

        public async Task<ExaminerQuiz> UpdateQuiz(ExaminerQuiz quiz)
        {
            var quizEntity = await dbContext.Quizzes.Where(q => q.Id == quiz.Id).FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Quiz not found");
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
                Questions = quiz.Questions.Select(q => new Models.Question
                {
                    Id = q.Id,
                    Statement = q.Statement,
                    EditorCode = q.EditorCode,
                    QuestionConfiguration = q.QuestionConfiguration,
                    TestCases = q.TestCases,
                    QuizId = q.QuizId,
                    Order = q.Order
                }).ToList()
            };

            var updatedQuiz = await quizzesRepository.UpdateQuizAsync(quizEntity);
            return updatedQuiz.ToExaminerQuiz();
        }
    }
}
