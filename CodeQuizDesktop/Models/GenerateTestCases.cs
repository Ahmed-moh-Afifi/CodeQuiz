namespace CodeQuizDesktop.Models
{
    /// <summary>
    /// Request model for generating test cases using AI.
    /// </summary>
    public class GenerateTestCasesRequest
    {
        /// <summary>
        /// The problem statement to generate test cases for.
        /// </summary>
        public required string ProblemStatement { get; set; }

        /// <summary>
        /// The programming language (e.g., "CSharp", "Python").
        /// </summary>
        public required string Language { get; set; }

        /// <summary>
        /// Existing test cases to avoid duplicates.
        /// </summary>
        public List<TestCase>? ExistingTestCases { get; set; }

        /// <summary>
        /// A sample/reference solution for validation (optional).
        /// </summary>
        public string? SampleSolution { get; set; }

        /// <summary>
        /// Number of test cases to generate (default: 5).
        /// </summary>
        public int? Count { get; set; }
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
