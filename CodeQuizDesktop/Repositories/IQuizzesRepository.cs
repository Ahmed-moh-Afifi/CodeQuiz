using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface IQuizzesRepository : ITwoTypesObservableRepository<ExaminerQuiz, ExamineeQuiz>
    {
        public Task<ExaminerQuiz> CreateQuiz(NewQuizModel newQuizModel);
        public Task<ExaminerQuiz> UpdateQuiz(int QuizId, NewQuizModel newQuizModel);
        public Task DeleteQuiz(int quizId);
        public Task<List<ExaminerQuiz>> GetUserQuizzes();
        public Task<List<ExaminerQuiz>> GetUserQuizzes(string userId);
        public Task<ExamineeQuiz> GetQuizByCode(string code);
        public Task<List<ExaminerAttempt>> GetQuizAttempts(int quizId);
        
        /// <summary>
        /// Join a quiz-specific SignalR group to receive real-time updates for a quiz.
        /// Used by examinees participating in a specific quiz.
        /// </summary>
        Task JoinQuizGroupAsync(int quizId);
        
        /// <summary>
        /// Leave a quiz-specific SignalR group when no longer participating in that quiz.
        /// </summary>
        Task LeaveQuizGroupAsync(int quizId);
    }
}
