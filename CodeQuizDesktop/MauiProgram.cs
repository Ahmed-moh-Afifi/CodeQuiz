using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Logging;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Resources;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using Microsoft.Extensions.Logging;
using Refit;
using Sharpnado.MaterialFrame;
using SkiaSharp.Views.Maui.Controls.Hosting;

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
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SFMono-Regular.otf", "SFMonoRegular");
                    fonts.AddFont("Inter-Regular.otf", "InterRegular");
                    fonts.AddFont("Inter-ExtraBold.otf", "InterExtraBold");
                    fonts.AddFont("Inter-Black.otf", "InterBlack");
                });

            // Apply the "RemoveBorder" logic to all relevant controls
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoBorder", (h, v) => RemoveBorder(h.PlatformView));
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoBorder", (h, v) => RemoveBorder(h.PlatformView));
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoBorder", (h, v) => RemoveBorder(h.PlatformView));
            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoBorder", (h, v) => RemoveBorder(h.PlatformView));
            Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("NoBorder", (h, v) => RemoveBorder(h.PlatformView));
            Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("NoBorder", (h, v) => RemoveButtonBorder(h.PlatformView, v));

            // Helper method to modify the Windows Native Control
            static void RemoveBorder(object platformView)
            {
#if WINDOWS
    if (platformView is Microsoft.UI.Xaml.Controls.Control nativeControl)
    {
        // 1. Remove the main border
        nativeControl.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        
        // 2. Remove the "Focus Ring" (The separate border that appears when focused)
        nativeControl.FocusVisualPrimaryThickness = new Microsoft.UI.Xaml.Thickness(0);
        nativeControl.FocusVisualSecondaryThickness = new Microsoft.UI.Xaml.Thickness(0);
        
        // 3. Force the border brush to transparent in all states
        nativeControl.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        
        // 4. Override the focus state border brushes for TextBox and RichEditBox
        if (nativeControl is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.Resources["TextControlBorderBrush"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            textBox.Resources["TextControlBorderBrushPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            textBox.Resources["TextControlBorderBrushFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            textBox.Resources["TextControlBorderBrushDisabled"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
        else if (nativeControl.GetType().Name == "MauiRichEditBox")
        {
            // RichEditBox is used for Editor - override its resources too
            nativeControl.Resources["TextControlBorderBrush"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            nativeControl.Resources["TextControlBorderBrushPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            nativeControl.Resources["TextControlBorderBrushFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            nativeControl.Resources["TextControlBorderBrushDisabled"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
    }
#endif
            }

            // Helper method to remove borders from buttons on Windows (except those with BorderWidth > 0)
            static void RemoveButtonBorder(object platformView, IView view)
            {
#if WINDOWS
    if (platformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
    {
        // Remove the focus visual (focus ring) for all buttons
        nativeButton.FocusVisualPrimaryThickness = new Microsoft.UI.Xaml.Thickness(0);
        nativeButton.FocusVisualSecondaryThickness = new Microsoft.UI.Xaml.Thickness(0);
        nativeButton.UseSystemFocusVisuals = false;
        
        // Only remove border for buttons that don't have an explicit border (e.g., DangerButton has BorderWidth="1")
        if (view is Button mauiButton && mauiButton.BorderWidth > 0)
        {
            // Keep the border for buttons that explicitly define one
            return;
        }
        
        // Remove border thickness for other buttons
        nativeButton.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        nativeButton.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }
#endif
            }

#if DEBUG
            builder.Logging.AddDebug();
#endif
            //Popups
            builder.Services.AddTransient<AddQuestionDialog>();

            // Views
            builder.Services.AddTransient<Startup>();
            builder.Services.AddTransient<Login>();
            builder.Services.AddTransient<Register>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CreateQuiz>();
            builder.Services.AddTransient<JoinQuiz>();
            builder.Services.AddTransient<ExaminerViewQuiz>();
            builder.Services.AddTransient<GradeAttempt>();
            builder.Services.AddTransient<ExamineeReviewQuiz>();
            builder.Services.AddTransient<ProfilePage>();

            //ViewModels
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
            builder.Services.AddTransient<ProfileViewModel>();

            //Services
            builder.Services.AddSingleton(SecureStorage.Default);
            builder.Services.AddSingleton<IPopupService, PopupService>();
            builder.Services.AddSingleton<ITokenService, TokenService>();
            builder.Services.AddSingleton<IAuthenticationRepository, AuthenticationRepository>();
            builder.Services.AddSingleton<IAttemptsRepository, AttemptsRepository>();
            builder.Services.AddSingleton<IQuizzesRepository, QuizzesRepository>();
            builder.Services.AddSingleton<IUsersRepository, UsersRepository>();
            builder.Services.AddSingleton<IExecutionRepository, ExecutionRepository>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IQuizDialogService, QuizDialogService>();
            builder.Services.AddTransient<AuthHandler>();
            builder.Services.AddTransient<LoggingHandler>();
            builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

            // UIService replaces AlertService and provides both alert and loading indicator functionality
            builder.Services.AddSingleton<UIService>();
            builder.Services.AddSingleton<IUIService>(sp => sp.GetRequiredService<UIService>());
            builder.Services.AddSingleton<IAlertService>(sp => sp.GetRequiredService<UIService>());

            builder.Services.AddSingleton<GlobalExceptionHandler>();

            // APIs
            var uri = Config.API;
            builder.Services.AddRefitClient<IAuthAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<LoggingHandler>();
            builder.Services.AddRefitClient<IAttemptsAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<LoggingHandler>();
            builder.Services.AddRefitClient<IQuizzesAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<LoggingHandler>();
            builder.Services.AddRefitClient<IUsersAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<LoggingHandler>();
            builder.Services.AddRefitClient<IExecutionAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<LoggingHandler>();
            builder.Services.AddRefitClient<IAiAPI>().ConfigureHttpClient(c => c.BaseAddress = new Uri(uri)).AddHttpMessageHandler<AuthHandler>().AddHttpMessageHandler<LoggingHandler>();

            var app = builder.Build();
            serviceProvider = app.Services;

            return app;
        }
    }
}
