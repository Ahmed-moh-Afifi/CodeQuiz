namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// Represents an evaluation job to be processed by the background service.
    /// </summary>
    public class EvaluationJob
    {
        public required int AttemptId { get; set; }
        public required string ExamineeId { get; set; }
        public required string ExaminerId { get; set; }
        public required int QuizId { get; set; }
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    }
}
