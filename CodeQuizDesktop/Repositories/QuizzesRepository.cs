using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public class QuizzesRepository(IQuizzesAPI quizzesAPI) : IQuizzesRepository
    {
        public async Task<ExaminerQuiz> CreateQuiz(NewQuizModel newQuizModel)
        {
            try
            {
                return (await quizzesAPI.CreateQuiz(newQuizModel)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task DeleteQuiz(int quizId)
        {
            try
            {
                await quizzesAPI.DeleteQuiz(quizId);
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<ExamineeQuiz> GetQuizByCode(string code)
        {
            try
            {
                return (await quizzesAPI.GetQuizByCode(code)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<List<ExaminerQuiz>> GetUserQuizzes()
        {
            try
            {
                return (await quizzesAPI.GetUserQuizzes()).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<List<ExaminerQuiz>> GetUserQuizzes(string userId)
        {
            try
            {
                return (await quizzesAPI.GetUserQuizzes(userId)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<ExaminerQuiz> UpdateQuiz(ExaminerQuiz quiz)
        {
            try
            {
                return (await quizzesAPI.UpdateQuiz(quiz.Id, quiz)).Data!; 
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }
    }
}
