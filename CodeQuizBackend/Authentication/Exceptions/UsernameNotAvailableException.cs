namespace CodeQuizBackend.Authentication.Exceptions
{
    public class UsernameNotAvailableException : AuthenticationException
    {
        public UsernameNotAvailableException() : base("The provided username is not available.")
        {
        }
    }
}
