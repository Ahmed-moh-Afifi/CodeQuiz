using CodeQuizDesktop.Logging;

namespace CodeQuizDesktop.Services;

public class GlobalExceptionHandler
{
    private readonly IAlertService _alertService;
    private readonly IAppLogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IAlertService alertService, IAppLogger<GlobalExceptionHandler> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    public void Initialize()
    {
        // Handle exceptions from background threads
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Handle exceptions from tasks
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // Handle exceptions from the MAUI UI thread (most common crash source)
#if WINDOWS
        Microsoft.UI.Xaml.Application.Current.UnhandledException += OnWindowsUnhandledException;
#elif ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidUnhandledException;
#elif IOS || MACCATALYST
        ObjCRuntime.Runtime.MarshalManagedException += OnAppleUnhandledException;
#endif
    }

    /// <summary>
    /// Checks if the exception is a known LiveCharts2 issue that can be safely ignored.
    /// This is a known bug in LiveCharts2 when charts are disposed during MAUI navigation.
    /// </summary>
    private static bool IsKnownLiveChartsException(Exception? exception)
    {
        if (exception == null) return false;
        
        // Check for the ContentPanel cast exception from LiveCharts2
        if (exception.Message.Contains("Unable to cast to ContentPanel") &&
            exception.StackTrace?.Contains("LiveChartsCore") == true)
        {
            return true;
        }
        
        // Check inner exceptions for AggregateException
        if (exception is AggregateException aggregateException)
        {
            return aggregateException.InnerExceptions.Any(IsKnownLiveChartsException);
        }
        
        return exception.InnerException != null && IsKnownLiveChartsException(exception.InnerException);
    }

#if WINDOWS
    private void OnWindowsUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Filter out known LiveCharts2 disposal exceptions
        if (IsKnownLiveChartsException(e.Exception))
        {
            _logger.LogWarning("Suppressed known LiveCharts2 disposal exception");
            e.Handled = true;
            return;
        }
        
        _logger.LogError("Windows unhandled exception", e.Exception);
        e.Handled = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await _alertService.ShowErrorAsync(e.Exception);
        });
    }
#endif

#if ANDROID
    private void OnAndroidUnhandledException(object? sender, Android.Runtime.RaiseThrowableEventArgs e)
    {
        // Filter out known LiveCharts2 disposal exceptions
        if (IsKnownLiveChartsException(e.Exception))
        {
            _logger.LogWarning("Suppressed known LiveCharts2 disposal exception");
            e.Handled = true;
            return;
        }
        
        _logger.LogError("Android unhandled exception", e.Exception);
        e.Handled = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await _alertService.ShowErrorAsync(e.Exception);
        });
    }
#endif

#if IOS || MACCATALYST
    private void OnAppleUnhandledException(object? sender, ObjCRuntime.MarshalManagedExceptionEventArgs e)
    {
        // Filter out known LiveCharts2 disposal exceptions
        if (IsKnownLiveChartsException(e.Exception))
        {
            _logger.LogWarning("Suppressed known LiveCharts2 disposal exception");
            e.ExceptionMode = ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;
            return;
        }
        
        _logger.LogError("Apple platform unhandled exception", e.Exception);
        e.ExceptionMode = ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await _alertService.ShowErrorAsync(e.Exception);
        });
    }
#endif

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            // Filter out known LiveCharts2 disposal exceptions
            if (IsKnownLiveChartsException(exception))
            {
                _logger.LogWarning("Suppressed known LiveCharts2 disposal exception");
                return;
            }
            
            _logger.LogError("Unhandled exception occurred", exception);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _alertService.ShowErrorAsync(exception);
            });
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // Filter out known LiveCharts2 disposal exceptions
        if (IsKnownLiveChartsException(e.Exception))
        {
            _logger.LogWarning("Suppressed known LiveCharts2 disposal exception");
            e.SetObserved();
            return;
        }
        
        _logger.LogError("Unobserved task exception", e.Exception);
        e.SetObserved(); // Prevent the app from crashing
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await _alertService.ShowErrorAsync(e.Exception.InnerException ?? e.Exception);
        });
    }

    public void Cleanup()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

#if WINDOWS
        if (Microsoft.UI.Xaml.Application.Current != null)
        {
            Microsoft.UI.Xaml.Application.Current.UnhandledException -= OnWindowsUnhandledException;
        }
#elif ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser -= OnAndroidUnhandledException;
#elif IOS || MACCATALYST
        ObjCRuntime.Runtime.MarshalManagedException -= OnAppleUnhandledException;
#endif
    }
}
