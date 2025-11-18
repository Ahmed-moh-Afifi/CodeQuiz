namespace CodeQuizBackend.Authentication.Exceptions
{
    public class InvalidCredentialsException : AuthenticationException
    {
        public InvalidCredentialsException() : base("The provided credentials are invalid.")
        {
        }
    }
}
