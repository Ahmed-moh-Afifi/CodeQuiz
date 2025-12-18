namespace CodeQuizBackend.Authentication.Exceptions
{
    public class InvalidTokenException : AuthenticationException
    {
        public InvalidTokenException() : base("Your session has expired. Please log in again.")
        {
        }
    }
}
