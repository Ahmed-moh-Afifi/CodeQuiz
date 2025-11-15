namespace CodeQuizBackend.Execution.Models
{
    public class CodeRunnerResult
    {
        public bool Success { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }
    }
}
