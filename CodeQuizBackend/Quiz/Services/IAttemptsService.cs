using CodeQuizBackend.Quiz.Models.DTOs;

namespace CodeQuizBackend.Quiz.Services
{
    public interface IAttemptsService
    {
        public Task<ExamineeAttempt> BeginAttempt(string quizCode, string examineeId);
        public Task<ExamineeAttempt> SubmitAttempt(int attemptId);
        public Task<SolutionDTO> UpdateSolution(SolutionDTO solution);
        /// <summary>
        /// Updates only the grade and evaluator for a solution.
        /// Used by instructors for manual grading, separate from student code saves.
        /// </summary>
        public Task<SolutionDTO> UpdateSolutionGrade(UpdateSolutionGradeRequest request);
        /// <summary>
        /// Batch updates grades and feedback for multiple solutions at once.
        /// Sends a single email notification to the examinee with all changes.
        /// </summary>
        public Task<List<SolutionDTO>> BatchUpdateSolutionGrades(BatchUpdateSolutionGradesRequest request);
        public Task<List<ExamineeAttempt>> GetExamineeAttempts(string examineeId);
        public Task EvaluateAttempt(int attemptId);
        /// <summary>
        /// Re-runs AI assessment for a specific solution.
        /// Used when the automatic assessment fails or needs to be re-evaluated.
        /// </summary>
        public Task<SolutionDTO> RerunAiAssessment(int solutionId);

        /// <summary>
        /// Gets a specific attempt by ID for the examiner view.
        /// </summary>
        public Task<ExaminerAttempt> GetAttemptById(int attemptId);
    }
}
