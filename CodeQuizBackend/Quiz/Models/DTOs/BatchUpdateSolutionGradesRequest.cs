namespace CodeQuizBackend.Quiz.Models.DTOs
{
    /// <summary>
    /// Request model for batch updating multiple solutions' grades.
    /// Used by instructors to update multiple grades at once with a single email notification.
    /// </summary>
    public class BatchUpdateSolutionGradesRequest
    {
        /// <summary>
        /// The attempt ID (used for validation and email notification)
        /// </summary>
        public required int AttemptId { get; set; }

        /// <summary>
        /// The list of solution grade updates
        /// </summary>
        public required List<SolutionGradeUpdate> Updates { get; set; }
    }

    /// <summary>
    /// Individual solution grade update within a batch request.
    /// </summary>
    public class SolutionGradeUpdate
    {
        /// <summary>
        /// The ID of the solution to grade
        /// </summary>
        public required int SolutionId { get; set; }

        /// <summary>
        /// The question number (order) for the email notification
        /// </summary>
        public int QuestionNumber { get; set; }

        /// <summary>
        /// The total points for the question (for email display)
        /// </summary>
        public float TotalPoints { get; set; }

        /// <summary>
        /// The grade awarded for this solution (null to leave unchanged)
        /// </summary>
        public float? ReceivedGrade { get; set; }

        /// <summary>
        /// The old/original grade before this update (for email notification)
        /// </summary>
        public float? OldGrade { get; set; }

        /// <summary>
        /// Feedback comment from the examiner for the student
        /// </summary>
        public string? Feedback { get; set; }

        /// <summary>
        /// The old/original feedback before this update (for email notification)
        /// </summary>
        public string? OldFeedback { get; set; }

        /// <summary>
        /// Identifier of who evaluated this solution (e.g., instructor name or "System")
        /// </summary>
        public string? EvaluatedBy { get; set; }
    }
}
