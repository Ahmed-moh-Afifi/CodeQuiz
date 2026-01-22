using CodeQuizBackend.Core.Data.models;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Execution.Models;
using CodeQuizBackend.Quiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuizBackend.Quiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController(IAiTestCaseGeneratorService testCaseGeneratorService, IAiAssessmentService assessmentService) : ControllerBase
    {
        /// <summary>
        /// Generates test cases for a question using AI.
        /// </summary>
        [HttpPost("generate-testcases")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<GeneratedTestCase>>>> GenerateTestCases([FromBody] GenerateTestCasesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProblemStatement))
            {
                throw new BadRequestException("Problem statement is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Language))
            {
                throw new BadRequestException("Language is required.");
            }

            var testCases = await testCaseGeneratorService.GenerateTestCasesAsync(
                request.ProblemStatement,
                request.Language,
                request.ExistingTestCases,
                request.SampleSolution,
                request.Count ?? 5);

            return Ok(new ApiResponse<List<GeneratedTestCase>>
            {
                Success = true,
                Data = testCases,
                Message = $"Generated {testCases.Count} test cases successfully."
            });
        }
    }

    /// <summary>
    /// Request model for generating test cases.
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
}
