using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Execution.Services
{
    public abstract class CodeRunnerDecorator(ICodeRunner innerRunner) : ICodeRunner
    {
        protected ICodeRunner innerRunner = innerRunner;
        public virtual string Language { get => innerRunner.Language; }

        public virtual Task<CodeRunnerResult> RunCodeAsync(string code, CodeRunnerOptions? options = null)
        {
            return innerRunner.RunCodeAsync(code, options);
        }
    }
}
