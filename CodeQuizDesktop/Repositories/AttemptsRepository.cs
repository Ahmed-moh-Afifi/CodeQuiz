using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public class AttemptsRepository(IAttemptsAPI attemptsAPI) : IAttemptsRepository
    {
        public async Task<ExamineeAttempt> BeginAttempt(BeginAttemptRequest beginAttemptRequest)
        {
            try
            {
                return (await attemptsAPI.BeginAttempt(beginAttemptRequest)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<List<ExamineeAttempt>> GetUserAttempts()
        {
            try
            {
                return (await attemptsAPI.GetUserAttempts()).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<ExamineeAttempt> SubmitAttempt(int attemptId)
        {
            try
            {
                return (await attemptsAPI.SubmitAttempt(attemptId)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<Solution> UpdateSolution(Solution solution)
        {
            try
            {
                return (await attemptsAPI.UpdateSolution(solution)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }
    }
}
