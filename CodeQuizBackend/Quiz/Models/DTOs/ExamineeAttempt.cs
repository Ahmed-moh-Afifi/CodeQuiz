namespace CodeQuizBackend.Quiz.Models.DTOs
{
    public class ExamineeAttempt
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
        public float? GradePercentage
        {
            get
            {
                if (Grade == null || Quiz.TotalPoints == 0)
                {
                    return null;
                }
                return (Grade / Quiz.TotalPoints) * 100;
            }
        }
        public required ExamineeQuiz Quiz { get; set; }
        public required List<SolutionDTO> Solutions { get; set; }
        public DateTime MaxEndTime { get => StartTime.AddMinutes(Quiz.Duration.TotalMinutes) <= Quiz.EndDate ? StartTime.AddMinutes(Quiz.Duration.TotalMinutes) : Quiz.EndDate; }

        public static ExamineeAttempt FromModel(Attempt attempt)
        {
            return new ExamineeAttempt
            {
                Id = attempt.Id,
                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime,
                QuizId = attempt.QuizId,
                ExamineeId = attempt.ExamineeId,
                Quiz = attempt.Quiz.ToExamineeQuiz(),
                Solutions = attempt.Solutions.Select(s => s.ToDTO()).ToList()
            };
        }
    }
}
