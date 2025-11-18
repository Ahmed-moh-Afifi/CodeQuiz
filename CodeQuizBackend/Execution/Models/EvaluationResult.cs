namespace CodeQuizBackend.Execution.Models
{
    public class EvaluationResult
    {
        public required TestCase TestCase { get; set; }
        public required string Output { get; set; }
        public required bool IsSuccessful { get; set; }
    }
}
