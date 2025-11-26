using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public class AuthenticationRepository(IAuthAPI authAPI, ITokenService tokenService, IUsersRepository usersRepository) : IAuthenticationRepository
    {
        private User? loggedInUser;
        public User? LoggedInUser { get => loggedInUser; set => loggedInUser = value; }

        public async Task ForgotPassword(ForgetPasswordModel forgetPasswordModel)
        {
            try
            {
                await authAPI.ForgetPasswordRequest(forgetPasswordModel);
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            try
            {
                var loginResult = (await authAPI.Login(loginModel)).Data;
                loggedInUser = loginResult?.User;
                return loginResult!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task Logout()
        {
            try
            {
                await tokenService.DeleteSavedTokens();
                loggedInUser = null;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<User> Register(RegisterModel registerModel)
        {
            try
            {
                var user = (await authAPI.Register(registerModel)).Data;
                return user!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            try
            {
                await authAPI.ResetPassword(resetPasswordModel);
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }
    }
}
