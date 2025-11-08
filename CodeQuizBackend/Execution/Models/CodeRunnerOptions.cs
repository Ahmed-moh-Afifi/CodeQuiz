namespace CodeQuizBackend.Execution.Models
{
    public class CodeRunnerOptions
    {
        public bool ContainOutput { get; set; } = false;
        public bool ContainError { get; set; } = false;
        public List<string> Input { get; set; } = [];
    }
}
