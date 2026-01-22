using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Core.Services.Ai;
using CodeQuizBackend.Execution.Models;
using System.Text;
using System.Text.Json;

namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// AI-powered test case generation service using Groq LLM.
    /// Generates diverse test cases including edge cases, boundary values, and normal cases.
    /// </summary>
    public class AiTestCaseGeneratorService : IAiTestCaseGeneratorService
    {
        private readonly IGroqService _groqService;
        private readonly IAppLogger<AiTestCaseGeneratorService> _logger;

        private const string SystemPrompt = @"You are an expert test case designer for a coding examination platform. Your task is to generate comprehensive test cases for programming problems.

When generating test cases, consider:
1. Normal/typical input values
2. Edge cases (empty input, single element, minimum/maximum values)
3. Boundary values (just below/above limits)
4. Special cases specific to the problem
5. Cases that might catch common implementation errors
6. Cases that detect hardcoding (use diverse inputs)

Each test case should have:
- Input: A list of input values (each line of input as a separate string)
- ExpectedOutput: The exact expected output string
- Description: What this test case is testing
- Category: One of ""Normal"", ""EdgeCase"", ""BoundaryValue"", ""Special"", ""AntiHardcode""

Respond ONLY with a JSON array in this exact format:
[
  {
    ""input"": [""line1"", ""line2""],
    ""expectedOutput"": ""expected result"",
    ""description"": ""Tests normal case with...''"",
    ""category"": ""Normal"",
    ""confidence"": 0.95
  }
]

Make sure:
- Input is an array of strings (each representing a line of stdin)
- ExpectedOutput is the exact expected stdout (trimmed)
- Generate diverse test cases to maximize code coverage
- If a sample solution is provided, you can verify your expected outputs make sense";

        public AiTestCaseGeneratorService(IGroqService groqService, IAppLogger<AiTestCaseGeneratorService> logger)
        {
            _groqService = groqService;
            _logger = logger;
        }

        public async Task<List<GeneratedTestCase>> GenerateTestCasesAsync(
            string problemStatement,
            string language,
            List<TestCase>? existingTestCases = null,
            string? sampleSolution = null,
            int count = 5,
            CancellationToken cancellationToken = default)
        {
            var userPrompt = BuildUserPrompt(problemStatement, language, existingTestCases, sampleSolution, count);

            try
            {
                _logger.LogInfo($"Generating {count} test cases for problem");
                var response = await _groqService.ChatCompletionAsync(SystemPrompt, userPrompt, cancellationToken);
                var testCases = ParseTestCasesResponse(response);
                _logger.LogInfo($"Generated {testCases.Count} test cases successfully");
                return testCases;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to generate test cases: {ex.Message}");
                throw;
            }
        }

        private static string BuildUserPrompt(
            string problemStatement,
            string language,
            List<TestCase>? existingTestCases,
            string? sampleSolution,
            int count)
        {
            var sb = new StringBuilder();

            sb.AppendLine("## Problem Statement");
            sb.AppendLine(problemStatement);
            sb.AppendLine();

            sb.AppendLine($"## Programming Language: {language}");
            sb.AppendLine();

            if (existingTestCases != null && existingTestCases.Count > 0)
            {
                sb.AppendLine("## Existing Test Cases (avoid duplicates)");
                foreach (var tc in existingTestCases)
                {
                    sb.AppendLine($"- Input: [{string.Join(", ", tc.Input.Select(i => $"\"{i}\""))}], Output: \"{tc.ExpectedOutput}\"");
                }
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(sampleSolution))
            {
                sb.AppendLine("## Reference Solution (for validation)");
                sb.AppendLine("```" + language.ToLower());
                sb.AppendLine(sampleSolution);
                sb.AppendLine("```");
                sb.AppendLine();
            }

            sb.AppendLine($"## Task");
            sb.AppendLine($"Generate exactly {count} diverse test cases for this problem. Include a mix of normal cases, edge cases, and boundary values. Respond with JSON array only.");

            return sb.ToString();
        }

        private List<GeneratedTestCase> ParseTestCasesResponse(string response)
        {
            try
            {
                // Try to extract JSON array from the response
                var jsonStart = response.IndexOf('[');
                var jsonEnd = response.LastIndexOf(']');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    response = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var parsed = JsonSerializer.Deserialize<List<TestCaseResponse>>(response, options)
                    ?? throw new JsonException("Deserialized to null");

                var testCases = new List<GeneratedTestCase>();
                for (int i = 0; i < parsed.Count; i++)
                {
                    var tc = parsed[i];
                    testCases.Add(new GeneratedTestCase
                    {
                        TestCase = new TestCase
                        {
                            TestCaseNumber = i + 1,
                            Input = tc.Input ?? [],
                            ExpectedOutput = tc.ExpectedOutput ?? ""
                        },
                        Description = tc.Description ?? "No description",
                        Category = tc.Category ?? "Normal",
                        Confidence = Math.Clamp(tc.Confidence, 0f, 1f)
                    });
                }

                return testCases;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning($"Failed to parse test cases response as JSON: {ex.Message}. Response: {response}");
                throw new InvalidOperationException($"Failed to parse AI-generated test cases: {ex.Message}");
            }
        }

        /// <summary>
        /// Internal class for deserializing the AI response.
        /// </summary>
        private class TestCaseResponse
        {
            public List<string>? Input { get; set; }
            public string? ExpectedOutput { get; set; }
            public string? Description { get; set; }
            public string? Category { get; set; }
            public float Confidence { get; set; }
        }
    }
}
