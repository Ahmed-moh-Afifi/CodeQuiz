using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Execution.Services
{
    public interface IEvaluator
    {
        Task<bool> EvaluateAsync(string language, string code, TestCase testCase);
    }
}
