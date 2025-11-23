using CodeQuizDesktop.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Services
{
    public interface ITokenService
    {
        public Task SaveTokens(TokenModel tokenModel);
        public Task<TokenModel?> GetValidTokens();
        public Task DeleteSavedTokens();
        public Task<bool> HasValidTokens();
    }
}
