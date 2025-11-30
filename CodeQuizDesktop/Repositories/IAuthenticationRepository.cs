using CodeQuizDesktop.Models;
using CodeQuizDesktop.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface IAuthenticationRepository
    {
        public User? LoggedInUser { get; set; }
        
        public Task<LoginResult> Login(LoginModel loginModel);
        public Task<User> Register(RegisterModel registerModel);
        public Task ResetPassword(ResetPasswordModel resetPasswordModel);
        public Task ForgotPassword(ForgetPasswordModel forgetPasswordModel);
        public Task Logout();
    }
}
