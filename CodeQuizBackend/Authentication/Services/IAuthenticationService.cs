using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;

namespace CodeQuizBackend.Authentication.Services
{
    public interface IAuthenticationService
    {
        public Task<UserDTO> Register(RegisterModel registerModel);
        public Task<LoginResult> Login(LoginModel loginModel);
        public Task<TokenModel> RefreshToken(TokenModel oldTokenModel);
        public Task ResetPassword(ResetPasswordModel resetPasswordModel);
        public Task ForgetPassword(ForgetPasswordModel forgetPasswordModel);
        public Task ResetPasswordWithToken(ResetPasswordTnModel resetPasswordTnModel);
    }
}
