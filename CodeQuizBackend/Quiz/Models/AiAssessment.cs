namespace CodeQuizBackend.Quiz.Models
{
    /// <summary>
    /// Represents the AI assessment of a solution, evaluating validity beyond test case results.
    /// </summary>
    public class AiAssessment
    {
        public required int Id { get; set; }

        /// <summary>
        /// The solution this assessment belongs to.
        /// </summary>
        public required int SolutionId { get; set; }

        /// <summary>
        /// Whether the AI considers the solution to be a valid, legitimate solution.
        /// False indicates potential issues like hardcoding, gaming test cases, etc.
        /// </summary>
        public required bool IsValid { get; set; }

        /// <summary>
        /// Confidence score from 0.0 to 1.0 indicating how confident the AI is in its assessment.
        /// </summary>
        public required float ConfidenceScore { get; set; }

        /// <summary>
        /// Detailed reasoning explaining the AI's assessment.
        /// Can be shown to students if instructor enables feedback visibility.
        /// </summary>
        public required string Reasoning { get; set; }

        /// <summary>
        /// List of flags/issues identified in the solution.
        /// Examples: "Hardcoded", "PartialImplementation", "IneffcientSolution", "PossiblePlagiarism"
        /// </summary>
        public List<string> Flags { get; set; } = [];

        /// <summary>
        /// When this assessment was performed.
        /// </summary>
        public required DateTime AssessedAt { get; set; }

        /// <summary>
        /// The AI model used for the assessment (e.g., "llama-3.3-70b-versatile").
        /// </summary>
        public required string Model { get; set; }

        /// <summary>
        /// AI-suggested grade for this solution based on code quality and correctness.
        /// Expressed as a percentage (0.0 to 1.0) of the question's total points.
        /// </summary>
        public float? SuggestedGrade { get; set; }

        /// <summary>
        /// Navigation property to the solution.
        /// </summary>
        public virtual Solution Solution { get; set; } = null!;
    }
}
