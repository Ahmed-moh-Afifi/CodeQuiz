using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;

namespace CodeQuizBackend.Quiz.Models.DTOs
{
    public class ExamineeQuiz
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required TimeSpan Duration { get; set; }
        public required string Code { get; set; }
        public required string ExaminerId { get; set; }
        public required QuestionConfiguration GlobalQuestionConfiguration { get; set; }
        public required bool AllowMultipleAttempts { get; set; }
        public required List<QuestionDTO> Questions { get; set; }
        public required UserDTO Examiner { get; set; }
        public int QustionsCount { get => Questions.Count; }
        public float TotalPoints { get => Questions.Sum(q => q.Points); }
    }
}
