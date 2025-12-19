using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace CodeQuizDesktop.Logging;

public class AppLogger<T>(ILogger<T> logger) : IAppLogger<T>
{
    public void LogDebug(string message, [CallerMemberName] string? methodName = null)
    {
        logger.LogDebug("[{Class}]::{Method} | {Message}", typeof(T).Name, methodName, message);
    }

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
