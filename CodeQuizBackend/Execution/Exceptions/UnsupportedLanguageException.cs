namespace CodeQuizBackend.Execution.Exceptions
{
    public class UnsupportedLanguageException(string language) : Exception($"The programming language '{language}' is not currently supported.")
    {
    }
}
