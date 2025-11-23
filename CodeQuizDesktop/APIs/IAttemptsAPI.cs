using CodeQuizDesktop.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.APIs
{
    public interface IAttemptsAPI
    {
        [Post("/Attempts/begin")]
        public Task<Models.ApiResponse<ExamineeAttempt>> BeginAttempt([Body] BeginAttemptRequest beginAttemptRequest);

        [Post("/Attempts/{attemptId}/submit")]
        public Task<Models.ApiResponse<ExamineeAttempt>> SubmitAttempt(int attemptId);

        [Put("/Attempts/solutions")]
        public Task<Models.ApiResponse<Solution>> UpdateSolution([Body] Solution solution);

        [Get("/Attempts/user")]
        public Task<Models.ApiResponse<List<ExamineeAttempt>>> GetUserAttempts();
    }
}
