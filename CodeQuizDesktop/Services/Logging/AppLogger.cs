using System.Runtime.CompilerServices;

namespace CodeQuizDesktop.Services.Logging;

public class AppLogger<T>
{
    private readonly string _logFile;

    public AppLogger()
    {
        _logFile = Path.Combine(FileSystem.AppDataDirectory, "app_log.txt");
    }

    private void WriteLog(string level, string message, string? methodName)
    {
        var log = $"{DateTime.Now} [{level}] [{typeof(T).Name}]::{methodName} | {message}{Environment.NewLine}";
        File.AppendAllText(_logFile, log);
    }

    public void LogInfo(string message, [CallerMemberName] string? methodName = null)
        => WriteLog("INFO", message, methodName);

    public void LogWarning(string message, [CallerMemberName] string? methodName = null)
        => WriteLog("WARN", message, methodName);

    public void LogError(string message, Exception? ex = null, [CallerMemberName] string? methodName = null)
    {
        var fullMsg = $"{message}\nException: {ex?.Message}\n{ex?.StackTrace}";
        WriteLog("ERROR", fullMsg, methodName);
    }
}
