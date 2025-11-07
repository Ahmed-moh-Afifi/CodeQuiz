using System.Runtime.CompilerServices;

namespace CodeQuizBackend.Core.Logging
{
    public class AppLogger<T>(ILogger<T> logger) : IAppLogger<T>
    {
        public void LogInfo(string message, [CallerMemberName] string? methodName = null)
        {
            logger.LogInformation("[{Class}]::{Method} | {Message}", typeof(T).Name, methodName, message);
        }

        public void LogWarning(string message, [CallerMemberName] string? methodName = null)
        {
            logger.LogWarning("[{Class}]::{Method} | {Message}", typeof(T).Name, methodName, message);
        }

        public void LogError(string message, Exception? ex = null, [CallerMemberName] string? methodName = null)
        {
            logger.LogError(ex, "[{Class}]::{Method} | {Message}", typeof(T).Name, methodName, message);
        }
    }
}
