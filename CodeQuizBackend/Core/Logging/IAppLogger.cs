using System.Runtime.CompilerServices;

namespace CodeQuizBackend.Core.Logging
{
    public interface IAppLogger<T>
    {
        void LogInfo(string message, [CallerMemberName] string? methodName = null);
        void LogWarning(string message, [CallerMemberName] string? methodName = null);
        void LogError(string message, Exception? ex = null, [CallerMemberName] string? methodName = null);
    }
}
