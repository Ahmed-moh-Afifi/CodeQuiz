using CodeQuizDesktop.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.APIs
{
    public interface IQuizzesAPI
    {
        [Post("/Quizzes")]
        public Task<Models.ApiResponse<ExaminerQuiz>> CreateQuiz([Body] NewQuizModel newQuizModel);

        [Put("/Quizzes/{id}")]
        public Task<Models.ApiResponse<ExaminerQuiz>> UpdateQuiz(int id, [Body] ExaminerQuiz examinerQuiz);

        [Delete("/Quizzes/{id}")]
        public Task<Models.ApiResponse<object>> DeleteQuiz(int id);

        [Get("/Quizzes/user")]
        public Task<Models.ApiResponse<List<ExaminerQuiz>>> GetUserQuizzes();

        [Get("/Quizzes/user/{id}")]
        public Task<Models.ApiResponse<List<ExaminerQuiz>>> GetUserQuizzes(string id);

        [Get("/Quizzes/code/{code}")]
        public Task<Models.ApiResponse<ExamineeQuiz>> GetQuizByCode(string code);

        [Get("/Quizzes/{quizId}/attempts")]
        public Task<Models.ApiResponse<List<ExaminerAttempt>>> GetQuizAttempts(int quizId);
    }
}
