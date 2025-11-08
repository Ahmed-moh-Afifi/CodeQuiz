namespace CodeQuizBackend.Execution.Services
{
    public class CodeRunnerFactory : ICodeRunnerFactory
    {
        private readonly Dictionary<string, ICodeRunner> codeRunners;

        public CodeRunnerFactory(IEnumerable<ICodeRunner> codeRunners)
        {
            this.codeRunners = codeRunners.ToDictionary(r => r.GetType().Name.Replace("CodeRunner", "").ToLower(), r => r);
        }

        public ICodeRunner Create(string language)
        {
            if (codeRunners.TryGetValue(language.ToLower(), out var codeRunner))
            {
                return codeRunner;
            }
            throw new NotSupportedException($"Language '{language}' is not supported.");
        }
    }
}
