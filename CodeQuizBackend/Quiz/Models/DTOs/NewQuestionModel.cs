using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Quiz.Models.DTOs
{
    public class NewQuestionModel
    {
        public required string Statement { get; set; }
        public required string EditorCode { get; set; }
        public QuestionConfiguration? QuestionConfiguration { get; set; }
        public required List<TestCase> TestCases { get; set; }
        public required int Order { get; set; }

        public Models.Question ToQuestion()
        {
            return new Models.Question
            {
                Id = 0,
                Statement = Statement,
                EditorCode = EditorCode,
                QuestionConfiguration = QuestionConfiguration,
                TestCases = TestCases,
                QuizId = 0,
                Order = Order
            };
        }

        public Models.Question ToQuestion(int quizId)
        {
            var question = ToQuestion();
            question.QuizId = quizId;
            return question;
        }
    }
}