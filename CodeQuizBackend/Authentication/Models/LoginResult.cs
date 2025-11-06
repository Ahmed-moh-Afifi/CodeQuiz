using CodeQuizBackend.Authentication.Models.DTOs;

namespace CodeQuizBackend.Authentication.Models
{
    public class LoginResult
    {
        public required UserDTO User { get; set; }
        public required TokenModel TokenModel { get; set; }
    }
}
