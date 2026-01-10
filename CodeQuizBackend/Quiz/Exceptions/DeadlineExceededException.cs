namespace CodeQuizBackend.Quiz.Exceptions
{
    /// <summary>
    /// Exception thrown when a user attempts to save a solution after the deadline has passed.
    /// This enforces strict time limits to prevent exploitation of the grace buffer period.
    /// </summary>
    public class DeadlineExceededException(string message = "The deadline for this attempt has passed. Your solution cannot be saved.") : Exception(message)
    {
    }
}
