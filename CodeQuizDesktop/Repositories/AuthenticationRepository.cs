using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Services;

namespace CodeQuizDesktop.Repositories;

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
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to process password reset request.");
        }
    }

    public async Task<LoginResult> Login(LoginModel loginModel)
    {
        try
        {
            var loginResult = (await authAPI.Login(loginModel)).Data;
            await tokenService.SaveTokens(loginResult!.TokenModel);
            loggedInUser = loginResult?.User;
            return loginResult!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to log in.");
        }
    }

    public async Task Logout()
    {
        try
        {
            await tokenService.DeleteSavedTokens();
            loggedInUser = null;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to log out.");
        }
    }

    public async Task<User> Register(RegisterModel registerModel)
    {
        try
        {
            var user = (await authAPI.Register(registerModel)).Data;
            return user!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to register.");
        }
    }

    public async Task ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        try
        {
            await authAPI.ResetPassword(resetPasswordModel);
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to reset password.");
        }
    }
}
