using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models.Authentication
{
    public class TokenModel
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }

        private DateTime ExpiryFromToken()
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(AccessToken);

            return jwt.ValidTo.ToLocalTime();
        }

        public bool IsValid()
        {
            return ExpiryFromToken().Subtract(DateTime.Now).TotalMinutes > 5;
        }
    }

    [JsonSerializable(typeof(TokenModel))]
    public partial class TokenJsonContext : JsonSerializerContext
    {
    }
}
