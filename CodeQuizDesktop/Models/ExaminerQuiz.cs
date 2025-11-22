using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class ExaminerQuiz
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
        public required List<Question> Questions { get; set; }
        public required int QustionsCount { get; set; }
        public required int AttemptsCount { get; set; }
        public required int SubmittedAttemptsCount { get; set; }
        public required float AverageAttemptScore { get; set; }
        public required float TotalPoints { get; set; }
    }
}
