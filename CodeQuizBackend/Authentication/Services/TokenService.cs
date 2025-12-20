using CodeQuizBackend.Authentication.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CodeQuizBackend.Authentication.Services
{
    public class TokenService(IConfiguration configuration, ILogger<TokenService> logger) : ITokenService
    {
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            logger.LogDebug("TokenService -> GenerateAccessToken: generating access token");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = configuration["Issuer"],
                Audience = configuration["Audience"],
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(double.Parse(configuration["JWTExpiresInMinutes"]!)),
                SigningCredentials = creds
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            logger.LogDebug("TokenService -> GenerateRefreshToken: generating refresh token");
            return Guid.NewGuid().ToString();
        }

        public IEnumerable<Claim> GetPrincipalFromExpiredToken(string token)
        {
            logger.LogDebug("TokenService -> GetPrincipalFromExpiredToken: getting principal from expired token");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Issuer"],
                ValidAudience = configuration["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTKey"]!))
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                _ = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new InvalidTokenException();

                return jwtSecurityToken.Claims;
            }
            catch (SecurityTokenException)
            {
                throw new InvalidTokenException();
            }
        }
    }
}
