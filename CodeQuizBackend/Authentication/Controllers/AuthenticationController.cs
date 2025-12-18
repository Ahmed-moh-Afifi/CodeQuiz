using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Authentication.Services;
using CodeQuizBackend.Core.Data.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CodeQuizBackend.Authentication.Controllers
{
    [EnableRateLimiting("StrictPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody] RegisterModel model)
        {
            return Ok(new ApiResponse<UserDTO>()
            {
                Success = true,
                Data = await authenticationService.Register(model),
                Message = "User registered successfully"
            });
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse<LoginResult>>> Login([FromBody] LoginModel model)
        {
            return Ok(new ApiResponse<LoginResult>()
            {
                Success = true,
                Data = await authenticationService.Login(model),
                Message = "User logged in successfully"
            });
        }

        [HttpPost("Refresh")]
        public async Task<ActionResult<ApiResponse<TokenModel>>> Refresh([FromBody] TokenModel model)
        {
            return Ok(new ApiResponse<TokenModel>()
            {
                Success = true,
                Data = await authenticationService.RefreshToken(model),
                Message = "Token refreshed successfully"
            });
        }

        [HttpPut]
        [Route("ResetPassword")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {
            await authenticationService.ResetPassword(resetPasswordModel);
            return Ok(new ApiResponse<object>()
            {
                Success = true,
                Data = null,
                Message = "Password reset successfully"
            });
        }

        [HttpPost]
        [Route("ForgetPasswordRequest")]
        public async Task<ActionResult<ApiResponse<object>>> ForgetPasswordRequest([FromBody] ForgetPasswordModel forgetPasswordModel)
        {
            await authenticationService.ForgetPassword(forgetPasswordModel);
            return Ok(new ApiResponse<object>()
            {
                Success = true,
                Data = null,
                Message = "If the email exists, a password reset link has been sent."
            });
        }

        [HttpPut]
        [Route("ResetPasswordTn")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPasswordTn([FromBody] ResetPasswordTnModel resetPasswordTnModel)
        {
            await authenticationService.ResetPasswordWithToken(resetPasswordTnModel);
            return Ok(new ApiResponse<object>()
            {
                Success = true,
                Data = null,
                Message = "Password has been reset successfully."
            });
        }
    }
}
