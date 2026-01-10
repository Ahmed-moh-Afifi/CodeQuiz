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
        public Task<List<ExamineeAttempt>> GetExamineeAttempts(string examineeId);
        public Task EvaluateAttempt(int attemptId);
    }
}
