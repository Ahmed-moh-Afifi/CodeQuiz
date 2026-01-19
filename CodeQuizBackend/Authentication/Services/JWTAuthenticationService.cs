using CodeQuizBackend.Authentication.Exceptions;
using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CodeQuizBackend.Authentication.Services
{
    public class JWTAuthenticationService(UserManager<User> userManager, ITokenService tokenService, ApplicationDbContext dbContext, IConfiguration configuration, IMailService mailService) : IAuthenticationService
    {
        public async Task ForgetPassword(ForgetPasswordModel forgetPasswordModel)
        {
            var user = await userManager.FindByEmailAsync(forgetPasswordModel.Email);

            // Always return success message to prevent email enumeration attacks
            if (user == null)
            {
                return;
            }

            var resetPassToken = await userManager.GeneratePasswordResetTokenAsync(user);
            if (resetPassToken == null)
            {
                // Log internally but don't expose to user
                return;
            }

            // URL-encode the token to handle special characters like + and /
            var encodedToken = Uri.EscapeDataString(resetPassToken);
            var resetLink = configuration["ForgetPasswordWebsiteUrl"] + encodedToken;
            var firstName = user.FirstName ?? user.UserName ?? "User";

            await mailService.SendPasswordResetEmailAsync(forgetPasswordModel.Email, firstName, resetLink);
        }

        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            var user = await userManager.FindByNameAsync(loginModel.Username);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                throw new InvalidCredentialsException();
            }

            var claims = new[]
            {
                new Claim("uid", user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var accessToken = tokenService.GenerateAccessToken(claims);
            var refreshToken = tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(double.Parse(configuration["RefreshTokenExpiresInDays"]!))
            };

            dbContext.RefreshTokens.Add(refreshTokenEntity);

            await dbContext.SaveChangesAsync();

            var tokenModel = new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return new LoginResult
            {
                User = user.ToDTO(),
                TokenModel = tokenModel
            };
        }

        public async Task<TokenModel> RefreshToken(TokenModel oldTokenModel)
        {
            var claims = tokenService.GetPrincipalFromExpiredToken(oldTokenModel.AccessToken);
            var userId = claims.First(c => c.Type == "uid").Value;

            var savedRefreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == oldTokenModel.RefreshToken && rt.UserId == userId);

            if (savedRefreshToken == null || savedRefreshToken.IsRevoked || savedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                throw new InvalidTokenException();
            }

            var newAccessToken = tokenService.GenerateAccessToken(claims);
            var newRefreshToken = tokenService.GenerateRefreshToken();

            savedRefreshToken.Token = newRefreshToken;
            savedRefreshToken.ExpiryDate = DateTime.UtcNow.AddDays(double.Parse(configuration["RefreshTokenExpiresInDays"]!));
            await dbContext.SaveChangesAsync();

            return new TokenModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<UserDTO> Register(RegisterModel registerModel)
        {
            // Check if username is already taken
            var existingUserByUsername = await userManager.FindByNameAsync(registerModel.Username);
            if (existingUserByUsername != null)
            {
                throw new UsernameNotAvailableException();
            }

            // Check if email is already registered
            var existingUserByEmail = await userManager.FindByEmailAsync(registerModel.Email);
            if (existingUserByEmail != null)
            {
                throw new EmailAlreadyRegisteredException();
            }

            var user = User.FromRegisterModel(registerModel);
            var result = await userManager.CreateAsync(user, registerModel.Password);
            if (!result.Succeeded)
            {
                // Check for password-related errors
                if (result.Errors.Any(e => e.Code.Contains("Password")))
                {
                    throw new WeakPasswordException();
                }

                // Generic registration failure without exposing internal details
                throw new AuthenticationException("Registration failed. Please check your information and try again.");
            }
            await mailService.SendWelcomeEmailAsync(registerModel.Email, user.FirstName);
            return user.ToDTO();
        }

        public async Task ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            var user = await userManager.FindByNameAsync(resetPasswordModel.Username);
            if (user == null || !await userManager.CheckPasswordAsync(user, resetPasswordModel.Password))
            {
                throw new InvalidCredentialsException();
            }

            var resetPassToken = await userManager.GeneratePasswordResetTokenAsync(user)
                ?? throw new AuthenticationException("Unable to reset password. Please try again later.");

            var result = await userManager.ResetPasswordAsync(user, resetPassToken, resetPasswordModel.NewPassword);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code.Contains("Password")))
                {
                    throw new WeakPasswordException();
                }
                throw new AuthenticationException("Unable to reset password. Please try again later.");
            }
        }

        public async Task ResetPasswordWithToken(ResetPasswordTnModel resetPasswordTnModel)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordTnModel.Email)
                ?? throw new AuthenticationException("Unable to reset password. The reset link may have expired.");

            var result = await userManager.ResetPasswordAsync(user, resetPasswordTnModel.Token, resetPasswordTnModel.NewPassword);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code.Contains("Password")))
                {
                    throw new WeakPasswordException();
                }
                if (result.Errors.Any(e => e.Code.Contains("Token")))
                {
                    throw new AuthenticationException("The password reset link has expired or is invalid. Please request a new one.");
                }
                throw new AuthenticationException("Unable to reset password. Please try again later.");
            }
        }
    }
}
