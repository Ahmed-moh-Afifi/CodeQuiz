namespace CodeQuizDesktop.Models
{
    /// <summary>
    /// Request model for updating a solution's grade.
    /// Used by instructors for manual grading, separate from student code saves.
    /// </summary>
    public class UpdateSolutionGradeRequest
    {
        /// <summary>
        /// The ID of the solution to grade
        /// </summary>
        public required int SolutionId { get; set; }

        /// <summary>
        /// The grade awarded for this solution (null to leave unchanged)
        /// </summary>
        public float? ReceivedGrade { get; set; }

        /// <summary>
        /// Identifier of who evaluated this solution (e.g., instructor name or "System")
        /// </summary>
        public string? EvaluatedBy { get; set; }

        /// <summary>
        /// Feedback comment from the examiner for the student
        /// </summary>
        public string? Feedback { get; set; }
    }
}
