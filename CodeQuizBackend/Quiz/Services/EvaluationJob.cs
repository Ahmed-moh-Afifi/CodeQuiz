namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// Represents an evaluation job to be processed by the background service.
    /// </summary>
    public class EvaluationJob
    {
        public int AttemptId { get; set; }
        public string ExamineeId { get; set; } = string.Empty;
        public string ExaminerId { get; set; } = string.Empty;
        public int QuizId { get; set; }
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// If set, indicates this is an AI-only reassessment for a specific solution.
        /// </summary>
        public int? SpecificSolutionId { get; set; }

        /// <summary>
        /// Whether this is an AI-only reassessment job (skips system grading).
        /// </summary>
        public bool IsAiReassessmentOnly => SpecificSolutionId.HasValue;

        public EvaluationJob() { }

        public EvaluationJob(int attemptId, int quizId, string examineeId, string examinerId, int? specificSolutionId = null)
        {
            AttemptId = attemptId;
            QuizId = quizId;
            ExamineeId = examineeId;
            ExaminerId = examinerId;
            SpecificSolutionId = specificSolutionId;
        }
    }
}
