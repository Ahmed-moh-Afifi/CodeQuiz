namespace CodeQuizBackend.Execution.Exceptions
{
    public class CodeRunnerException(string message = "Failed to run code") : Exception(message)
    {
    }
}
