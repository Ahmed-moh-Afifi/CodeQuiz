using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Execution.Services
{
    public class Evaluator(ICodeRunnerFactory codeRunnerFactory) : IEvaluator
    {
        public async Task<EvaluationResult> EvaluateAsync(string language, string code, TestCase testCase)
        {
            var codeRunner = codeRunnerFactory.Create(language);
            var result = await codeRunner.RunCodeAsync(code, new CodeRunnerOptions
            {
                Input = testCase.Input,
                ContainOutput = true,
                ContainError = true
            });

            return new EvaluationResult
            {
                TestCase = testCase,
                Output = result.Output ?? string.Empty,
                IsSuccessful = result.Success && (result.Output?.Trim() == testCase.ExpectedOutput.Trim())
            };
        }
    }
}
