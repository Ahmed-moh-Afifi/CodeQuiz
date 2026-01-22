using CodeQuizBackend.Execution.Models;
using CodeQuizBackend.Quiz.Models;

namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// Interface for AI-powered test case generation service.
    /// </summary>
    public interface IAiTestCaseGeneratorService
    {
        /// <summary>
        /// Generates test cases for a question using AI based on the problem statement.
        /// </summary>
        /// <param name="problemStatement">The problem/question statement.</param>
        /// <param name="language">The programming language for the question.</param>
        /// <param name="existingTestCases">Existing test cases to avoid duplicates (optional).</param>
        /// <param name="sampleSolution">A sample/reference solution if available (optional).</param>
        /// <param name="count">Number of test cases to generate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of generated test cases for instructor review.</returns>
        Task<List<GeneratedTestCase>> GenerateTestCasesAsync(
            string problemStatement,
            string language,
            List<TestCase>? existingTestCases = null,
            string? sampleSolution = null,
            int count = 5,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a generated test case with additional metadata for instructor review.
    /// </summary>
    public class GeneratedTestCase
    {
        /// <summary>
        /// The actual test case data.
        /// </summary>
        public required TestCase TestCase { get; set; }

        /// <summary>
        /// AI's description of what this test case is testing.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The category of the test case (e.g., "EdgeCase", "Normal", "BoundaryValue", "Performance").
        /// </summary>
        public required string Category { get; set; }

        /// <summary>
        /// AI's confidence that this is a valid and useful test case.
        /// </summary>
        public required float Confidence { get; set; }
    }
}
