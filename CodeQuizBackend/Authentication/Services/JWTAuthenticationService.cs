using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CodeQuizBackend.Authentication.Services
{
    public class JWTAuthenticationService(UserManager<User> userManager, TokenService tokenService, ApplicationDbContext dbContext, IConfiguration configuration, IMailService mailService) : IAuthenticationService
    {
        public async Task ForgetPassword(ForgetPasswordModel forgetPasswordModel)
        {
            var user = await userManager.FindByEmailAsync(forgetPasswordModel.Email) ?? throw new Exception("Failed to serve your request");

            var resetPassToken = await userManager.GeneratePasswordResetTokenAsync(user)
                ?? throw new Exception("Failed to serve your request");

            var resetLink = configuration["ForgetPasswordWebsiteUrl"] + resetPassToken;
            var firstName = user.FirstName ?? user.UserName ?? "User";

            await mailService.SendPasswordResetEmailAsync(forgetPasswordModel.Email, firstName, resetLink);
        }

        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            var user = await userManager.FindByNameAsync(loginModel.Username);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                // TODO: Create custom exception types
                throw new Exception("Invalid username or password");
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
                // TODO: Create custom exception types
                throw new SecurityTokenException("Invalid refresh token");
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
            var user = User.FromRegisterModel(registerModel);
            var result = await userManager.CreateAsync(user, registerModel.Password);
            if (!result.Succeeded)
            {
                // TODO: Create custom exception types
                throw new Exception("User registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            await mailService.SendWelcomeEmailAsync(registerModel.Email, user.FirstName);
            return user.ToDTO();
        }

        public async Task ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            var user = await userManager.FindByNameAsync(resetPasswordModel.Username);
            if (user == null || !await userManager.CheckPasswordAsync(user, resetPasswordModel.Password))
            {
                // TODO: Create custom exception types
                throw new Exception("Invalid username or password");
            }

            var resetPassToken = await userManager.GeneratePasswordResetTokenAsync(user)
                ?? throw new Exception("Failed to reset password");

            var result = await userManager.ResetPasswordAsync(user, resetPassToken, resetPasswordModel.NewPassword);
            if (!result.Succeeded)
            {
                // TODO: Create custom exception types
                throw new Exception("Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task ResetPasswordWithToken(ResetPasswordTnModel resetPasswordTnModel)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordTnModel.Email)
                ?? throw new Exception("Failed to reset password");

            var result = await userManager.ResetPasswordAsync(user, resetPasswordTnModel.Token, resetPasswordTnModel.NewPassword);
            if (!result.Succeeded)
            {
                // TODO: Create custom exception types
                throw new Exception("Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
