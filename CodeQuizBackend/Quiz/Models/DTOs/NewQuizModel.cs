namespace CodeQuizBackend.Quiz.Models.DTOs
{
    public class NewQuizModel
    {
        public required string Title { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required TimeSpan Duration { get; set; }
        public required string ExaminerId { get; set; }
        public required QuestionConfiguration GlobalQuestionConfiguration { get; set; }
        public required bool AllowMultipleAttempts { get; set; }
        public required List<NewQuestionModel> Questions { get; set; }

        private Models.Quiz ToQuiz()
        {
            return new Models.Quiz
            {
                Id = 0,
                Title = Title,
                StartDate = StartDate,
                EndDate = EndDate,
                Duration = Duration,
                Code = string.Empty,
                ExaminerId = ExaminerId,
                GlobalQuestionConfiguration = GlobalQuestionConfiguration,
                AllowMultipleAttempts = AllowMultipleAttempts,
                Questions = Questions.Select(q => q.ToQuestion()).ToList(),
                TotalPoints = Questions.Sum(q => q.Points)
            };
        }

        public Models.Quiz ToQuiz(string generatedCode)
        {
            var quiz = ToQuiz();
            quiz.Code = generatedCode;
            return quiz;
        }

        public List<string> Validate()
        {
            List<string> errors = [];
            if (Questions.Count == 0)
            {
                errors.Add("Cannot create quiz without questions");
            }

            return errors;
        }
    }
}
