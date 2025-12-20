using CodeQuizDesktop.Logging;
using CodeQuizDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.APIs
{
    public class AuthHandler(ITokenService tokenService, IAppLogger<AuthHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await tokenService.GetValidTokens() ?? throw new Exception("Session Expired"); // Replace with a custom exception
            logger.LogInfo($"Sending request using token: {token.AccessToken}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }

}
