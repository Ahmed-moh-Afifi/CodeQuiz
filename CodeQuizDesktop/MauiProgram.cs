using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Sharpnado.MaterialFrame;
using CodeQuizDesktop.Services.Logging;
using CodeQuizDesktop.Services.Exceptions;
using CodeQuizDesktop.Services.UI;

namespace CodeQuizDesktop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSharpnadoMaterialFrame(loggerEnable: false)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(typeof(AppLogger<>));
        builder.Services.AddSingleton<GlobalExceptionHandler>();
        builder.Services.AddSingleton<ISnackBarService, SnackBarService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        app.Services.GetService<GlobalExceptionHandler>();

        var logger = app.Services.GetService<AppLogger<GlobalExceptionHandler>>();
        logger?.LogInfo("✅ App started with Snackbar support");

        return app;
    }
}
