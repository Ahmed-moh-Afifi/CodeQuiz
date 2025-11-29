using System;
using CodeQuizDesktop.Services.Logging;

namespace CodeQuizDesktop.Services.Exceptions;

public class GlobalExceptionHandler
{
    private readonly AppLogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(AppLogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;

        AppDomain.CurrentDomain.UnhandledException += HandleUnhandled;
    }

    private void HandleUnhandled(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        _logger.LogError("Unhandled exception", ex);
    }
}
