namespace CodeQuizBackend.Execution.Models
{
    public class TestCase
    {
        public required List<string> Input { get; set; }
        public required string ExpectedOutput { get; set; }
    }
}
