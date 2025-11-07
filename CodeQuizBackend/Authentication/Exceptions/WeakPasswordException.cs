namespace CodeQuizBackend.Authentication.Exceptions
{
    public class WeakPasswordException : AuthenticationException
    {
        public WeakPasswordException() : base("The provided password is too weak. Make sure it contains at least 8 characters, including uppercase, lowercase, numbers, and special symbols.")
        {
        }
    }
}
