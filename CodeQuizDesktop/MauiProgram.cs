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
        static IServiceProvider serviceProvider;

        public static TService GetService<TService>()
            => serviceProvider.GetService<TService>();
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

            builder.Services.AddTransient<AddQuestionDialog, AddQuestionDialogVM>();
            builder.Services.AddScoped<QuizSettingsDialog, QuizSettingsDialogVM>();

            builder.Services.AddTransient<LoginVM>();
            builder.Services.AddTransient<RegisterVM>();
            builder.Services.AddTransient<DashboardVM>();
            builder.Services.AddTransient<CreatedQuizzesVM>();
            builder.Services.AddTransient<JoinedQuizzesVM>();
            builder.Services.AddTransient<CreateQuizVM>();
            builder.Services.AddTransient<JoinQuizVM>();
            builder.Services.AddTransient<ExaminerViewQuizVM>();
            builder.Services.AddTransient<StartupViewModel>();
            builder.Services.AddTransient<GradeAttemptVM>();
            builder.Services.AddTransient<ExamineeReviewQuizVM>();
            builder.Services.AddTransient<EditQuizVM>();

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


            var app = builder.Build();
            serviceProvider = app.Services;

            return app;
        }
    }
}
