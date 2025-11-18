using CodeQuizBackend.Quiz.Models.DTOs;

namespace CodeQuizBackend.Quiz.Services
{
    public interface IAttemptsService
    {
        public Task<ExamineeAttempt> BeginAttempt(string quizCode, string examineeId);
        public Task<ExamineeAttempt> SubmitAttempt(int attemptId);
        public Task<SolutionDTO> UpdateSolution(SolutionDTO solution);
        public Task<List<ExamineeAttempt>> GetExamineeAttempts(string examineeId);
    }
}
