using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using Microsoft.Extensions.Logging;
using Refit;
using Sharpnado.MaterialFrame;

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


#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddScoped<AddQuestionDialog, AddQuestionDialogVM>();
            builder.Services.AddScoped<QuizSettingsDialog, QuizSettingsDialogVM>();

            builder.Services.AddScoped<LoginVM>();
            builder.Services.AddScoped<RegisterVM>();
            builder.Services.AddScoped<DashboardVM>();
            builder.Services.AddScoped<CreatedQuizzesVM>();
            builder.Services.AddScoped<JoinedQuizzesVM>();
            builder.Services.AddScoped<CreateQuizVM>();
            builder.Services.AddScoped<JoinQuizVM>();
            builder.Services.AddScoped<ExaminerViewQuizVM>();
            builder.Services.AddScoped<StartupViewModel>();

            builder.Services.AddSingleton<IPopupService, PopupService>();
            builder.Services.AddSingleton<ITokenService, TokenService>();

            var uri = "http://localhost:5062/api";

            builder.Services.AddRefitClient<IAuthAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri));
            builder.Services.AddRefitClient<IAttemptsAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>();
            builder.Services.AddRefitClient<IQuizzesAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>();
            builder.Services.AddRefitClient<IUsersAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>();
            builder.Services.AddRefitClient<IExecutionAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>();

            builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
            builder.Services.AddSingleton<IAuthenticationRepository, AuthenticationRepository>();
            builder.Services.AddSingleton<IAttemptsRepository, AttemptsRepository>();
            builder.Services.AddSingleton<IQuizzesRepository, QuizzesRepository>();
            builder.Services.AddSingleton<IUsersRepository, UsersRepository>();
            builder.Services.AddSingleton<IExecutionRepository, ExecutionRepository>();
            builder.Services.AddTransient<AuthHandler>();


            return builder.Build();
        }
    }
}
