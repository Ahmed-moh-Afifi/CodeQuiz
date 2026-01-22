namespace CodeQuizDesktop.Models
{
    /// <summary>
    /// Payload for evaluation status events from SignalR.
    /// </summary>
    public class EvaluationStatusPayload
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public ExaminerAttempt? ExaminerAttempt { get; set; }
        public ExamineeAttempt? ExamineeAttempt { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
