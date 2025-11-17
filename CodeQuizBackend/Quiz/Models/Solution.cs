using CodeQuizBackend.Execution.Models;
using CodeQuizBackend.Quiz.Models.DTOs;

namespace CodeQuizBackend.Quiz.Models
{
    public class Solution
    {
        public required int Id { get; set; }
        public required string Code { get; set; }
        public required int QuestionId { get; set; }
        public required int AttemptId { get; set; }
        public List<EvaluationResult>? EvaluationResults { get; set; }

        public virtual Question Question { get; set; } = null!;
        public virtual Attempt Attempt { get; set; } = null!;

        public SolutionDTO ToDTO()
        {
            return new() 
            {
                Id = Id,
                QuestionId = QuestionId,
                AttemptId = AttemptId,
                Code = Code,
                EvaluationResults = EvaluationResults
            };
        }
    }
}
