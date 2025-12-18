namespace CodeQuizBackend.Authentication.Exceptions
{
    public class EmailAlreadyRegisteredException : AuthenticationException
    {
        public EmailAlreadyRegisteredException() : base("An account with this email address already exists.")
        {
        }
    }
}
