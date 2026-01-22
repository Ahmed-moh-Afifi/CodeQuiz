namespace CodeQuizDesktop.Models
{
    public class Solution
    {
        public required int Id { get; set; }
        public required string Code { get; set; }
        public required int QuestionId { get; set; }
        public required int AttemptId { get; set; }
        public string? EvaluatedBy { get; set; }
        public float? ReceivedGrade { get; set; }
        public List<EvaluationResult>? EvaluationResults { get; set; }

        /// <summary>
        /// Feedback comment from the instructor about the solution.
        /// </summary>
        public string? Feedback { get; set; }

        /// <summary>
        /// AI assessment of the solution. May be null if AI assessment hasn't run yet,
        /// or if the instructor has disabled AI feedback visibility for students.
        /// </summary>
        public AiAssessment? AiAssessment { get; set; }
    }
}
