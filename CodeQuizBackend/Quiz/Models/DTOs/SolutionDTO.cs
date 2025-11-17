using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Quiz.Models.DTOs
{
    public class SolutionDTO
    {
        public required int Id { get; set; }
        public required string Code { get; set; }
        public required int QuestionId { get; set; }
        public required int AttemptId { get; set; }
        public List<EvaluationResult>? EvaluationResults { get; set; }
    }
}
