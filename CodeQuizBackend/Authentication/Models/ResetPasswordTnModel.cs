namespace CodeQuizBackend.Authentication.Models
{
    public class ResetPasswordTnModel : ForgetPasswordModel
    {
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
