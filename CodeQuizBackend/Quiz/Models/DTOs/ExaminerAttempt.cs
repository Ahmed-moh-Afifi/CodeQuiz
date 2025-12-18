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
                Solutions = attempt.Solutions.Select(s => s.ToDTO()).ToList(),
                Examinee = attempt.Examinee.ToDTO()
            };
        }
    }
}
