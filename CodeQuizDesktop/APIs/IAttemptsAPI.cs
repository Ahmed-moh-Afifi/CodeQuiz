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

        /// <summary>
        /// Updates a solution's code (student saving their work)
        /// </summary>
        [Put("/Attempts/solutions")]
        public Task<Models.ApiResponse<Solution>> UpdateSolution([Body] Solution solution);

        /// <summary>
        /// Updates a solution's grade (instructor grading)
        /// </summary>
        [Put("/Attempts/solutions/grade")]
        public Task<Models.ApiResponse<Solution>> UpdateSolutionGrade([Body] UpdateSolutionGradeRequest request);

        /// <summary>
        /// Batch updates grades and feedback for multiple solutions at once.
        /// </summary>
        [Put("/Attempts/solutions/grades/batch")]
        public Task<Models.ApiResponse<List<Solution>>> BatchUpdateSolutionGrades([Body] BatchUpdateSolutionGradesRequest request);

        /// <summary>
        /// Re-runs AI assessment for a specific solution.
        /// </summary>
        [Post("/Attempts/solutions/{solutionId}/ai-reassess")]
        public Task<Models.ApiResponse<Solution>> RerunAiAssessment(int solutionId);

        [Get("/Attempts/user")]
        public Task<Models.ApiResponse<List<ExamineeAttempt>>> GetUserAttempts();

        /// <summary>
        /// Gets a specific attempt by ID for the examiner view.
        /// </summary>
        [Get("/Attempts/{attemptId}")]
        public Task<Models.ApiResponse<ExaminerAttempt>> GetAttemptById(int attemptId);
    }
}
