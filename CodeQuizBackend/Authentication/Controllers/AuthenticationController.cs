using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuizBackend.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            return Ok(await authenticationService.Register(model));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            return Ok(await authenticationService.Login(model));
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenModel model)
        {
            return Ok(await authenticationService.RefreshToken(model));
        }

        [HttpPut]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {
            await authenticationService.ResetPassword(resetPasswordModel);
            return Ok();
        }

        [HttpPost]
        [Route("ForgetPasswordRequest")]
        public async Task<IActionResult> ForgetPasswordRequest([FromBody] ForgetPasswordModel forgetPasswordModel)
        {
            await authenticationService.ForgetPassword(forgetPasswordModel);
            return Ok();
        }

        [HttpPut]
        [Route("ResetPasswordTn")]
        public async Task<IActionResult> ResetPasswordTn([FromBody] ResetPasswordTnModel resetPasswordTnModel)
        {
            await authenticationService.ResetPasswordWithToken(resetPasswordTnModel);
            return Ok();
        }
    }
}
