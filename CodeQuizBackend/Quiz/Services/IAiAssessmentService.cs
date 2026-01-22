using CodeQuizBackend.Quiz.Models;

namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// Interface for AI-powered solution assessment service.
    /// </summary>
    public interface IAiAssessmentService
    {
        /// <summary>
        /// Assesses a solution using AI to determine its validity beyond test case results.
        /// Detects issues like hardcoded solutions, gaming test cases, partial implementations, etc.
        /// </summary>
        /// <param name="solution">The solution to assess (should include related Question and EvaluationResults).</param>
        /// <param name="question">The question the solution is for.</param>
        /// <param name="questionConfig">The configuration for the question (language, execution settings, etc.).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The AI assessment of the solution.</returns>
        Task<AiAssessment> AssessSolutionAsync(
            Solution solution,
            Question question,
            QuestionConfiguration questionConfig,
            CancellationToken cancellationToken = default);
    }
}
