using CodeQuizBackend.Execution.Exceptions;

namespace CodeQuizBackend.Execution.Services
{
    public class CodeRunnerFactory(IEnumerable<ICodeRunner> codeRunners, SandboxedCodeRunnerFactory sandboxFactory) : ICodeRunnerFactory
    {
        private readonly Dictionary<string, ICodeRunner> codeRunners = codeRunners.ToDictionary(r => r.Language.ToLower(), r => r);

        public ICodeRunner Create(string language, bool sandbox = true)
        {
            if (codeRunners.TryGetValue(language.ToLower(), out var codeRunner))
            {
                return sandbox ? sandboxFactory(codeRunner) : codeRunner;
            }
            throw new UnsupportedLanguageException(language);
        }

        public IEnumerable<string> GetSupportedLanguages()
        {
            return codeRunners.Values.Select(r => r.Language);
        }
    }
}
