using System.Security.Cryptography;

namespace CodeQuizBackend.Core.Utils
{
    public class SecureCodeGenerator
    {
        private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string GenerateSecureCode(int length)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return new string([.. bytes.Select(b => Characters[b % Characters.Length])]);
        }
    }
}
