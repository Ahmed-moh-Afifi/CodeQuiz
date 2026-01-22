namespace CodeQuizDesktop.Models
{
    /// <summary>
    /// Represents the AI assessment of a solution.
    /// </summary>
    public class AiAssessment
    {
        public required int Id { get; set; }
        public required int SolutionId { get; set; }

        /// <summary>
        /// Whether the AI considers the solution to be a valid, legitimate solution.
        /// </summary>
        public required bool IsValid { get; set; }

        /// <summary>
        /// Confidence score from 0.0 to 1.0.
        /// </summary>
        public required float ConfidenceScore { get; set; }

        /// <summary>
        /// Detailed reasoning explaining the AI's assessment.
        /// </summary>
        public required string Reasoning { get; set; }

        /// <summary>
        /// List of flags/issues identified (e.g., "Hardcoded", "PartialImplementation").
        /// </summary>
        public List<string> Flags { get; set; } = [];

        /// <summary>
        /// When this assessment was performed.
        /// </summary>
        public required DateTime AssessedAt { get; set; }

        /// <summary>
        /// When this assessment was performed, converted to local time.
        /// </summary>
        public DateTime AssessedAtLocal => AssessedAt.ToLocalTime();

        /// <summary>
        /// The AI model used for the assessment.
        /// </summary>
        public required string Model { get; set; }

        /// <summary>
        /// AI-suggested grade as a percentage (0.0 to 1.0) of the question's total points.
        /// </summary>
        public float? SuggestedGrade { get; set; }
    }
}
