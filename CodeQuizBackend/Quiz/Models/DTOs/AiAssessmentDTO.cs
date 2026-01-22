namespace CodeQuizBackend.Quiz.Models.DTOs
{
    /// <summary>
    /// DTO for AI assessment data.
    /// </summary>
    public class AiAssessmentDTO
    {
        public required int Id { get; set; }
        public required int SolutionId { get; set; }
        public required bool IsValid { get; set; }
        public required float ConfidenceScore { get; set; }
        public required string Reasoning { get; set; }
        public List<string> Flags { get; set; } = [];
        public required DateTime AssessedAt { get; set; }
        public required string Model { get; set; }

        /// <summary>
        /// AI-suggested grade as a percentage (0.0 to 1.0) of the question's total points.
        /// </summary>
        public float? SuggestedGrade { get; set; }
    }
}
