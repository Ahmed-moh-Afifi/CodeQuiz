using CodeQuizBackend.Execution.Exceptions;
using CodeQuizBackend.Execution.Models;

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

        public IEnumerable<SupportedLanguage> GetSupportedLanguages()
        {
            return codeRunners.Values.Select(r => new SupportedLanguage { Name = r.Language, Extension = r.Extension });
        }
    }
}
