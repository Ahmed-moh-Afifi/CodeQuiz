using CodeQuizBackend.Authentication.Models.DTOs;

namespace CodeQuizBackend.Quiz.Models.DTOs
{
    public class ExaminerAttempt
    {
        public required int Id { get; set; }
        public required DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } = null;
        public required int QuizId { get; set; }
        public required string ExamineeId { get; set; }
        public required float TotalPoints { get; set; }
        public float? Grade
        {
            get
            {
                if (Solutions.Any(s => s.ReceivedGrade == null))
                    return null;
                else
                    return Solutions.Sum(s => s.ReceivedGrade);
            }
        }
        public float? GradePercentage
        {
            get
            {
                if (Grade == null || TotalPoints == 0)
                {
                    return null;
                }
                return (Grade / TotalPoints) * 100;
            }
        }
        public required List<SolutionDTO> Solutions { get; set; }
        public required UserDTO Examinee { get; set; }

        public static ExaminerAttempt FromModel(Attempt attempt)
        {
            return new ExaminerAttempt
            {
                Id = attempt.Id,
                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime,
                QuizId = attempt.QuizId,
                ExamineeId = attempt.ExamineeId,
                TotalPoints = attempt.Quiz.Questions.Sum(q => q.Points),
                Solutions = attempt.Solutions.Select(s => s.ToDTO()).ToList(),
                Examinee = attempt.Examinee.ToDTO()
            };
        }
    }
}
