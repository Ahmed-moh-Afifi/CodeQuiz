namespace CodeQuizBackend.Quiz.Exceptions
{
    public class MultipleAttemptsNotAllowedException() : Exception("You have already completed this quiz. Multiple attempts are not allowed.")
    {
    }
}
