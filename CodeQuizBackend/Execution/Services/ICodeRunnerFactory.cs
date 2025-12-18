namespace CodeQuizBackend.Execution.Services
{
    public interface ICodeRunnerFactory
    {
        ICodeRunner Create(string language, bool sandbox = true);
        IEnumerable<string> GetSupportedLanguages();
    }

    public delegate ICodeRunner SandboxedCodeRunnerFactory(ICodeRunner innerRunner);
}
