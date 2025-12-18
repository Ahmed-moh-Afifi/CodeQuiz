namespace CodeQuizBackend.Quiz.Exceptions
{
    public class QuizNotActiveException(string message = "This quiz is not currently active.") : Exception(message)
    {
    }
}
