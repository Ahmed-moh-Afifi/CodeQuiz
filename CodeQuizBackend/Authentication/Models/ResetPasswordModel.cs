namespace CodeQuizBackend.Authentication.Models
{
    public class ResetPasswordModel : LoginModel
    {
        public required string NewPassword { get; set; }
    }
}
