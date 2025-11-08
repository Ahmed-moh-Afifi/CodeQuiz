namespace CodeQuizBackend.Execution.Services
{
    public interface ICodeRunnerFactory
    {
        ICodeRunner Create(string language);
    }
}
