using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Services
{
    class TokenService(IAuthAPI authAPI, ISecureStorage secureStorage) : ITokenService
    {
        private TokenModel? savedTokens = null;
        private Task<TokenModel?>? refreshTask = null;

        public async Task DeleteSavedTokens()
        {
            await Task.FromResult(secureStorage.Remove("token-model"));
            savedTokens = null;
        }

        public async Task<TokenModel?> GetValidTokens()
        {
            if (await HasValidTokens())
            {
                return savedTokens;
            }
            refreshTask = RefreshTokens();
            var refreshedTokens = await refreshTask;
            refreshTask = null;
            if (refreshedTokens != null)
            {
                savedTokens = refreshedTokens;
                await SaveTokens(refreshedTokens);
            }
            return refreshedTokens;
        }

        public async Task SaveTokens(TokenModel tokenModel)
        {
            await secureStorage.SetAsync("token-model", JsonSerializer.Serialize(tokenModel, TokenJsonContext.Default.TokenModel));
        }

        public async Task<bool> HasValidTokens()
        {
            if (savedTokens is null)
            {
                await ReadSavedTokens();
            }
            if (savedTokens is not null && savedTokens.IsValid())
            {
                return true;
            }
            return false;
        }

        private async Task<TokenModel?> ReadSavedTokens()
        {
            var tokenString = await secureStorage.GetAsync("token-model");
            if (string.IsNullOrWhiteSpace(tokenString))
                return null;
            savedTokens = JsonSerializer.Deserialize(tokenString, TokenJsonContext.Default.TokenModel);
            return savedTokens;
        }

        private async Task<TokenModel?> RefreshTokens()
        {
            if (refreshTask != null)
            {
                return await refreshTask;
            }

            if (savedTokens == null)
            {
                await ReadSavedTokens();
            }
            if (savedTokens != null)
            {
                try
                {
                    var newTokens = (await authAPI.Refresh(savedTokens)).Data;
                    return newTokens;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
    }
}
