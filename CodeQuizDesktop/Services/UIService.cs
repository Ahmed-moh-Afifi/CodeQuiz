using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Logging;
using CodeQuizDesktop.Models;
using System.Text.Json;

namespace CodeQuizDesktop.Services;

/// <summary>
/// Service for handling UI operations including alerts and loading indicators.
/// </summary>
public class UIService(IAppLogger<UIService> logger) : IUIService
{
    private bool _isLoading;
    private Page? _loadingOverlayPage;
    private readonly SemaphoreSlim _loadingSemaphore = new(1, 1);

    public bool IsLoading => _isLoading;

    #region Alert Methods

    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        if (Application.Current?.Windows.Count > 0)
        {
            var mainPage = Application.Current.Windows[0].Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert(title, message, cancel);
            }
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        if (Application.Current?.Windows.Count > 0)
        {
            var mainPage = Application.Current.Windows[0].Page;
            if (mainPage != null)
            {
                return await mainPage.DisplayAlert(title, message, accept, cancel);
            }
        }
        return false;
    }

    public async Task ShowErrorAsync(string message, string? title = null)
    {
        logger.LogError(message);
        await ShowAlertAsync(title ?? "Error", message);
    }

    public async Task ShowErrorAsync(Exception exception, string? userFriendlyMessage = null)
    {
        logger.LogError(userFriendlyMessage ?? exception.Message, exception);

        var message = userFriendlyMessage ?? GetUserFriendlyMessage(exception);
        await ShowAlertAsync("Error", message);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ApiServiceException apiServiceEx => apiServiceEx.UserMessage,
            HttpRequestException => "Unable to connect to the server. Please check your internet connection.",
            TaskCanceledException => "The request timed out. Please try again.",
            Refit.ValidationApiException validationEx => validationEx.Content?.ToString() ?? "Validation error occurred.",
            Refit.ApiException apiEx => ApiServiceException.FromApiException(apiEx).UserMessage,
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            _ => "An unexpected error occurred. Please try again."
        };
    }

    #endregion

    #region Loading Indicator Methods

    public async Task ShowLoadingAsync(string? message = null)
    {
        await _loadingSemaphore.WaitAsync();
        try
        {
            if (_isLoading)
                return;

            _isLoading = true;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    var window = Application.Current.Windows[0];
                    var currentPage = window.Page;

                    if (currentPage is Shell shell)
                    {
                        currentPage = shell.CurrentPage;
                    }

                    if (currentPage != null)
                    {
                        // Create and add the loading overlay
                        var loadingOverlay = CreateLoadingOverlay(message);
                        
                        if (currentPage is ContentPage contentPage && contentPage.Content is Layout layout)
                        {
                            // Wrap existing content in a grid with overlay
                            var existingContent = contentPage.Content;
                            var grid = new Grid();
                            grid.Children.Add(existingContent);
                            grid.Children.Add(loadingOverlay);
                            contentPage.Content = grid;
                            _loadingOverlayPage = contentPage;
                        }
                    }
                }
            });
        }
        finally
        {
            _loadingSemaphore.Release();
        }
    }

    public async Task HideLoadingAsync()
    {
        await _loadingSemaphore.WaitAsync();
        try
        {
            if (!_isLoading)
                return;

            _isLoading = false;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (_loadingOverlayPage is ContentPage contentPage && contentPage.Content is Grid grid && grid.Children.Count > 1)
                {
                    // Remove the overlay and restore original content
                    var originalContent = grid.Children[0];
                    grid.Children.Clear();
                    contentPage.Content = (View)originalContent;
                }
                _loadingOverlayPage = null;
            });
        }
        finally
        {
            _loadingSemaphore.Release();
        }
    }

    public async Task<T> ExecuteWithLoadingAsync<T>(Func<Task<T>> operation, string? loadingMessage = null)
    {
        try
        {
            await ShowLoadingAsync(loadingMessage);
            return await operation();
        }
        finally
        {
            await HideLoadingAsync();
        }
    }

    public async Task ExecuteWithLoadingAsync(Func<Task> operation, string? loadingMessage = null)
    {
        try
        {
            await ShowLoadingAsync(loadingMessage);
            await operation();
        }
        finally
        {
            await HideLoadingAsync();
        }
    }

    private static View CreateLoadingOverlay(string? message)
    {
        var overlay = new Grid
        {
            BackgroundColor = Color.FromArgb("#CC1e1e1e"),
            ZIndex = 1000
        };

        var container = new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Spacing = 20
        };

        var activityIndicator = new ActivityIndicator
        {
            IsRunning = true,
            Color = Color.FromArgb("#591c21"),
            WidthRequest = 50,
            HeightRequest = 50,
            HorizontalOptions = LayoutOptions.Center
        };

        container.Children.Add(activityIndicator);

        if (!string.IsNullOrEmpty(message))
        {
            var label = new Label
            {
                Text = message,
                TextColor = Color.FromArgb("#D4D4D4"),
                FontFamily = "InterRegular",
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center
            };
            container.Children.Add(label);
        }

        overlay.Children.Add(container);
        return overlay;
    }

    #endregion
}
