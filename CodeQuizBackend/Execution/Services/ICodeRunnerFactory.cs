using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Execution.Services
{
    public interface ICodeRunnerFactory
    {
        ICodeRunner Create(string language, bool sandbox = true);
        IEnumerable<SupportedLanguage> GetSupportedLanguages();
    }

    public delegate ICodeRunner SandboxedCodeRunnerFactory(ICodeRunner innerRunner);
}
