using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Execution.Services
{
    public interface ICodeRunner
    {
        Task<CodeRunnerResult> RunCodeAsync(string code, CodeRunnerOptions? options = null);
    }
}
