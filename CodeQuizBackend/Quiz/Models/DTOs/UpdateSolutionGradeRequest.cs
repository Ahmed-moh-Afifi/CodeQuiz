namespace CodeQuizBackend.Quiz.Models.DTOs
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
        /// The grade awarded for this solution
        /// </summary>
        public required float ReceivedGrade { get; set; }
        
        /// <summary>
        /// Identifier of who evaluated this solution (e.g., instructor name or "System")
        /// </summary>
        public string? EvaluatedBy { get; set; }
    }
}
