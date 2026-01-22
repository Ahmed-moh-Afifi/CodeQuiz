using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Core.Services.Ai;
using CodeQuizBackend.Quiz.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace CodeQuizBackend.Quiz.Services
{
    /// <summary>
    /// AI-powered solution assessment service using Groq LLM.
    /// Analyzes solutions to detect issues that test cases might miss.
    /// </summary>
    public class AiAssessmentService : IAiAssessmentService
    {
        private readonly IGroqService _groqService;
        private readonly GroqSettings _groqSettings;
        private readonly IAppLogger<AiAssessmentService> _logger;

        private const string SystemPrompt = @"You are an expert code reviewer for a coding examination platform. Your task is to assess whether a submitted solution is a valid, legitimate implementation that correctly solves the problem, and suggest an appropriate grade.

You must analyze the code and determine:
1. Whether the solution is a genuine implementation or if it's gaming the test cases (e.g., hardcoding expected outputs)
2. Whether the logic actually solves the stated problem or just happens to pass the given test cases
3. Whether there are any red flags suggesting the solution is not a proper implementation
4. What grade the solution deserves as a percentage (0.0 to 1.0) of the question's total points

You will be given:
- The problem statement
- The submitted code
- The editor configuration (language, whether execution/intellisense was available)
- The test cases and their results

Respond ONLY with a JSON object in this exact format:
{
  ""isValid"": true/false,
  ""confidenceScore"": 0.0-1.0,
  ""suggestedGrade"": 0.0-1.0,
  ""reasoning"": ""Your detailed explanation"",
  ""flags"": [""Flag1"", ""Flag2""]
}

Grade suggestion guidelines:
- 1.0 (100%): Perfect solution, clean implementation, handles all cases correctly
- 0.8-0.99: Good solution with minor issues (style, efficiency)
- 0.5-0.79: Partial solution, some cases work but has notable issues
- 0.2-0.49: Significant problems, barely functional
- 0.0-0.19: Fundamentally broken, hardcoded, or gaming test cases

Possible flags to use (only include relevant ones):
- ""Hardcoded"": Solution hardcodes specific input/output values instead of implementing logic
- ""PartialImplementation"": Solution only handles specific cases, not the general problem
- ""IncorrectLogic"": Logic doesn't match the problem requirements despite passing tests
- ""IneffcientSolution"": Extremely inefficient implementation that wouldn't scale
- ""PossiblePlagiarism"": Code appears to be copied (unusual patterns, comments, naming)
- ""EmptyOrStub"": Solution is essentially empty or just a stub
- ""TestCaseGaming"": Solution appears designed to pass specific test cases without solving the general problem

Be thorough but fair. A solution that passes all test cases and implements correct logic should receive a high grade even if it's not the most elegant.";

        public AiAssessmentService(IGroqService groqService, IOptions<GroqSettings> groqSettings, IAppLogger<AiAssessmentService> logger)
        {
            _groqService = groqService;
            _groqSettings = groqSettings.Value;
            _logger = logger;
        }

        public async Task<AiAssessment> AssessSolutionAsync(
            Solution solution,
            Question question,
            QuestionConfiguration questionConfig,
            CancellationToken cancellationToken = default)
        {
            var userPrompt = BuildUserPrompt(solution, question, questionConfig);

            try
            {
                _logger.LogInfo($"Assessing solution {solution.Id} for question {question.Id}");
                var response = await _groqService.ChatCompletionAsync(SystemPrompt, userPrompt, cancellationToken);
                var assessment = ParseAssessmentResponse(response, solution.Id);
                assessment.Model = _groqSettings.Model;
                _logger.LogInfo($"Assessment complete for solution {solution.Id}: IsValid={assessment.IsValid}, Confidence={assessment.ConfidenceScore}");
                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to assess solution {solution.Id}: {ex.Message}");
                // Return a neutral assessment on failure
                return new AiAssessment
                {
                    Id = 0,
                    SolutionId = solution.Id,
                    IsValid = true,
                    ConfidenceScore = 0,
                    Reasoning = $"AI assessment failed: {ex.Message}. Solution not flagged.",
                    Flags = ["AssessmentFailed"],
                    AssessedAt = DateTime.UtcNow,
                    Model = _groqSettings.Model
                };
            }
        }

        private static string BuildUserPrompt(Solution solution, Question question, QuestionConfiguration config)
        {
            var sb = new StringBuilder();

            sb.AppendLine("## Problem Statement");
            sb.AppendLine(question.Statement);
            sb.AppendLine();

            sb.AppendLine("## Editor Configuration");
            sb.AppendLine($"- Language: {config.Language}");
            sb.AppendLine($"- Execution Allowed: {config.AllowExecution}");
            sb.AppendLine($"- Intellisense Enabled: {config.AllowIntellisense}");
            sb.AppendLine($"- Signature Help Enabled: {config.AllowSignatureHelp}");
            sb.AppendLine($"- Show Output: {config.ShowOutput}");
            sb.AppendLine($"- Show Errors: {config.ShowError}");
            sb.AppendLine();

            sb.AppendLine("## Submitted Code");
            sb.AppendLine("```" + config.Language.ToLower());
            sb.AppendLine(solution.Code);
            sb.AppendLine("```");
            sb.AppendLine();

            sb.AppendLine("## Test Cases and Results");
            if (solution.EvaluationResults != null && solution.EvaluationResults.Count > 0)
            {
                foreach (var result in solution.EvaluationResults)
                {
                    sb.AppendLine($"### Test Case {result.TestCase.TestCaseNumber}");
                    sb.AppendLine($"- Input: {string.Join(", ", result.TestCase.Input)}");
                    sb.AppendLine($"- Expected Output: {result.TestCase.ExpectedOutput}");
                    sb.AppendLine($"- Actual Output: {result.Output}");
                    sb.AppendLine($"- Passed: {result.IsSuccessful}");
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("No test case results available.");
                if (question.TestCases.Count > 0)
                {
                    sb.AppendLine("Test cases defined for this question:");
                    foreach (var tc in question.TestCases)
                    {
                        sb.AppendLine($"- Test {tc.TestCaseNumber}: Input={string.Join(", ", tc.Input)}, Expected={tc.ExpectedOutput}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("## Your Task");
            sb.AppendLine("Analyze the submitted code and determine if it's a valid, legitimate solution to the problem. Respond with JSON only.");

            return sb.ToString();
        }

        private AiAssessment ParseAssessmentResponse(string response, int solutionId)
        {
            try
            {
                // Try to extract JSON from the response (in case there's extra text)
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    response = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var parsed = JsonSerializer.Deserialize<AiAssessmentResponse>(response, options)
                    ?? throw new JsonException("Deserialized to null");

                return new AiAssessment
                {
                    Id = 0,
                    SolutionId = solutionId,
                    IsValid = parsed.IsValid,
                    ConfidenceScore = Math.Clamp(parsed.ConfidenceScore, 0f, 1f),
                    Reasoning = parsed.Reasoning ?? "No reasoning provided",
                    Flags = parsed.Flags ?? [],
                    AssessedAt = DateTime.UtcNow,
                    Model = string.Empty, // Will be set by caller
                    SuggestedGrade = parsed.SuggestedGrade.HasValue ? Math.Clamp(parsed.SuggestedGrade.Value, 0f, 1f) : null
                };
            }
            catch (JsonException ex)
            {
                _logger.LogWarning($"Failed to parse AI response as JSON: {ex.Message}. Response: {response}");
                // Return a default assessment if parsing fails
                return new AiAssessment
                {
                    Id = 0,
                    SolutionId = solutionId,
                    IsValid = true,
                    ConfidenceScore = 0,
                    Reasoning = $"Failed to parse AI response. Raw response: {response}",
                    Flags = ["ParseError"],
                    AssessedAt = DateTime.UtcNow,
                    Model = string.Empty,
                    SuggestedGrade = null
                };
            }
        }

        /// <summary>
        /// Internal class for deserializing the AI response.
        /// </summary>
        private class AiAssessmentResponse
        {
            public bool IsValid { get; set; }
            public float ConfidenceScore { get; set; }
            public float? SuggestedGrade { get; set; }
            public string? Reasoning { get; set; }
            public List<string>? Flags { get; set; }
        }
    }
}
