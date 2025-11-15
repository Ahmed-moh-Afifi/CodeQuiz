using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Execution.Services
{
    public interface ICodeRunner
    {
        public string Language { get; }
        Task<CodeRunnerResult> RunCodeAsync(string code, CodeRunnerOptions? options = null);
    }
}
