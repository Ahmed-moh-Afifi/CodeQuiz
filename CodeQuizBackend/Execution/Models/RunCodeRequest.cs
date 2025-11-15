namespace CodeQuizBackend.Execution.Models
{
    public class RunCodeRequest
    {
        public required string Language { get; set; }
        public required string Code { get; set; }
        public required List<string> Input { get; set; }
        public required bool ContainOutput { get; set; }
        public required bool ContainError { get; set; }
    }
}
