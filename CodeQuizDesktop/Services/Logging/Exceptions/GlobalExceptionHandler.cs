using CodeQuizDesktop.Services.Logging;
using CodeQuizDesktop.Services.UI;

namespace CodeQuizDesktop.Services.Exceptions;

public class GlobalExceptionHandler
{
    private readonly AppLogger<GlobalExceptionHandler> _logger;
    private readonly ISnackBarService _snackBar;

    public GlobalExceptionHandler(
        AppLogger<GlobalExceptionHandler> logger,
        ISnackBarService snackBar)
    {
        _logger = logger;
        _snackBar = snackBar;

        AppDomain.CurrentDomain.UnhandledException += HandleUnhandled;
    }

    private async void HandleUnhandled(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;

        _logger.LogError("Unhandled exception", ex);

        await _snackBar.ShowAsync(
            "An unexpected error occurred ❌",
            SnackBarType.Error
        );
    }
}
