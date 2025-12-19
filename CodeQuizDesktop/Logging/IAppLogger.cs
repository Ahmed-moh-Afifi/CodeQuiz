using System.Runtime.CompilerServices;

namespace CodeQuizDesktop.Logging;

public interface IAppLogger<T>
{
    void LogDebug(string message, [CallerMemberName] string? methodName = null);
    void LogInfo(string message, [CallerMemberName] string? methodName = null);
    void LogWarning(string message, [CallerMemberName] string? methodName = null);
    void LogError(string message, Exception? ex = null, [CallerMemberName] string? methodName = null);
}
