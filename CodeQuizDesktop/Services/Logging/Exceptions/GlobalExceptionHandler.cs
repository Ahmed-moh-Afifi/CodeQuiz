using CodeQuizDesktop.Services.Logging;

namespace CodeQuizDesktop.Services.Exceptions;

public class GlobalExceptionHandler
{
    private readonly AppLogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(AppLogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;

        AppDomain.CurrentDomain.UnhandledException += HandleUnhandled;
        TaskScheduler.UnobservedTaskException += HandleUnobserved;
    }

    private void HandleUnhandled(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        _logger.LogError("Unhandled exception", ex);
    }

    private void HandleUnobserved(object sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.LogError("Unobserved task exception", e.Exception);
        e.SetObserved();
    }
}
