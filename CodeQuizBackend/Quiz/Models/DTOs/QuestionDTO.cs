using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Quiz.Models.DTOs
{
    public class QuestionDTO
    {
        public required int Id { get; set; }
        public required string Statement { get; set; }
        public required string EditorCode { get; set; }
        public required QuestionConfiguration QuestionConfiguration { get; set; }
        public required List<TestCase> TestCases { get; set; }
        public required int QuizId { get; set; }
        public required int Order { get; set; }
        public required float Points { get; set; }
    }
}
