using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Execution.Services
{
    public interface IEvaluator
    {
        Task<EvaluationResult> EvaluateAsync(string language, string code, TestCase testCase);
    }
}
