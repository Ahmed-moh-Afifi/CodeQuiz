using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface IAttemptsRepository
    {
        public Task<ExamineeAttempt> BeginAttempt(BeginAttemptRequest beginAttemptRequest);
        public Task<ExamineeAttempt> SubmitAttempt(int attemptId);
        public Task<Solution> UpdateSolution(Solution solution);
        public Task<List<ExamineeAttempt>> GetUserAttempts();
    }
}
