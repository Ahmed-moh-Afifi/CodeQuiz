using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Core.Utils;

namespace CodeQuizBackend.Quiz.Services
{
    public class QuizCodeGenerator(ApplicationDbContext dbContext, IAppLogger<QuizCodeGenerator> logger)
    {
        private const int baseLength = 6;
        private const int maxLength = 10;
        private const int maxAttemptsPerLength = 50;

        public async Task<string> GenerateUniqueQuizCode()
        {
            var length = baseLength;
            string code;
            while (length <= maxLength)
            {
                for (int attempt = 0; attempt < maxAttemptsPerLength; attempt++)
                {
                    code = SecureCodeGenerator.GenerateSecureCode(length);
                    if (IsCodeUnique(code))
                    {
                        return code;
                    }
                }
                length++;
                logger.LogWarning($"Failed to generate unique quiz code with length {length - 1}. Increasing length to {length}.");
            }
            throw new InvalidOperationException($"Unable to generate unique quiz code with {maxLength} characters!");
        }

        private bool IsCodeUnique(string code)
        {
            return !dbContext.Quizzes.Any(q => q.Code == code && q.EndDate >= DateTime.Now);
        }
    }
}
