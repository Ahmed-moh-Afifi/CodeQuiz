using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using Microsoft.Extensions.Logging;
using Sharpnado.MaterialFrame;
using CodeQuizDesktop.Services.Logging;
using CodeQuizDesktop.Services.Exceptions;

namespace CodeQuizDesktop
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseSharpnadoMaterialFrame(loggerEnable: false);
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SFMono-Regular.otf", "SFMonoRegular");
                    fonts.AddFont("Inter-Regular.otf", "InterRegular");
                    fonts.AddFont("Inter-ExtraBold.otf", "InterExtraBold");
                    fonts.AddFont("Inter-Black.otf", "InterBlack");
                });


            builder.Services.AddSingleton(typeof(AppLogger<>));
            builder.Services.AddSingleton<GlobalExceptionHandler>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            app.Services.GetService<GlobalExceptionHandler>();

            return app;
        }
    }
}
