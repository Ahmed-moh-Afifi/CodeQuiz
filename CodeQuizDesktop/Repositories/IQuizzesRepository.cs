using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface IQuizzesRepository
    {
        public Task<ExaminerQuiz> CreateQuiz(NewQuizModel newQuizModel);
        public Task<ExaminerQuiz> UpdateQuiz(ExaminerQuiz quiz);
        public Task DeleteQuiz(int quizId);
        public Task<List<ExaminerQuiz>> GetUserQuizzes();
        public Task<List<ExaminerQuiz>> GetUserQuizzes(string userId);
        public Task<ExamineeQuiz> GetQuizByCode(string code);
    }
}
