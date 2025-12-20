using System.Collections.Generic;
using System.Security.Claims;

namespace CodeQuizBackend.Authentication.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        IEnumerable<Claim> GetPrincipalFromExpiredToken(string token);
    }
}
