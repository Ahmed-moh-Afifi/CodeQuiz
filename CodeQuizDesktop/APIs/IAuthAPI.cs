using CodeQuizDesktop.Models;
using CodeQuizDesktop.Models.Authentication;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.APIs
{
    public interface IAuthAPI
    {
        [Post("/Authentication/Register")]
        Task<Models.ApiResponse<User>> Register([Body] RegisterModel registerModel);

        [Post("/Authentication/Login")]
        Task<Models.ApiResponse<LoginResult>> Login([Body] LoginModel loginModel);

        [Post("/Authentication/Refresh")]
        Task<Models.ApiResponse<TokenModel>> Refresh([Body] TokenModel refreshModel);

        [Put("/Authentication/ResetPassword")]
        Task<Models.ApiResponse<object>> ResetPassword([Body] ResetPasswordModel resetPasswordModel);

        [Post("/Authentication/ForgetPasswordRequest")]
        Task<Models.ApiResponse<object>> ForgetPasswordRequest([Body] ForgetPasswordModel forgetPasswordModel);
    }
}
