using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Quiz.Models.DTOs;

namespace CodeQuizBackend.Quiz.Models
{
    public class Attempt
    {
        public required int Id { get; set; }
        public required DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } = null;
        public required int QuizId { get; set; }
        public required string ExamineeId { get; set; }

        public virtual Quiz Quiz { get; set; } = null!;
        public virtual List<Solution> Solutions { get; set; } = [];
        public virtual User Examinee { get; set; } = null!;

        public ExamineeAttempt ToExamineeAttempt()
        {
            return new ExamineeAttempt
            {
                Id = Id,
                StartTime = StartTime,
                EndTime = EndTime,
                QuizId = QuizId,
                ExamineeId = ExamineeId,
                Quiz = Quiz.ToExamineeQuiz(),
                Solutions = Solutions.Select(s => s.ToDTO()).ToList()
            };
        }

        public ExaminerAttempt ToExaminerAttempt()
        {
            return new ExaminerAttempt
            {
                Id = Id,
                StartTime = StartTime,
                EndTime = EndTime,
                QuizId = QuizId,
                ExamineeId = ExamineeId,
                Solutions = Solutions.Select(s => s.ToDTO()).ToList(),
                Examinee = Examinee.ToDTO()
            };
        }
    }
}
