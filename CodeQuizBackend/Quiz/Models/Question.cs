using CodeQuizBackend.Execution.Models;
using CodeQuizBackend.Quiz.Models.DTOs;

namespace CodeQuizBackend.Quiz.Models
{
    public class Question
    {
        public required int Id { get; set; }
        public required string Statement { get; set; }
        public required string EditorCode { get; set; }
        public QuestionConfiguration? QuestionConfiguration { get; set; }
        public List<TestCase> TestCases { get; set; } = [];
        public required int QuizId { get; set; }
        public required int Order { get; set; }

        public virtual Quiz Quiz { get; set; } = null!;

        public QuestionDTO ToDTO(QuestionConfiguration questionConfiguration)
        {
            return new QuestionDTO
            {
                Id = Id,
                Statement = Statement,
                EditorCode = EditorCode,
                QuestionConfiguration = questionConfiguration,
                TestCases = TestCases,
                QuizId = QuizId,
                Order = Order
            };
        }
    }
}
