using CodeQuizBackend.Quiz.Models.DTOs;

namespace CodeQuizBackend.Quiz.Services
{
    public interface IQuizzesService
    {
        public Task<ExaminerQuiz> CreateQuiz(NewQuizModel newQuizModel);
        public Task<ExaminerQuiz> UpdateQuiz(int id, NewQuizModel newQuizModel);
        public Task<List<ExaminerQuiz>> GetUserQuizzes(string userId);
        public Task DeleteQuiz(int id);
        public Task<ExamineeQuiz> GetQuizByCode(string code);
        public Task<List<ExaminerAttempt>> GetQuizAttempts(int quizId);
    }
}
