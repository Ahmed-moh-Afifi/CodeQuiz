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
    private Grid? _loadingOverlay;
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
                var rootPage = GetRootPage();
                if (rootPage == null)
                    return;

                _loadingOverlay = CreateLoadingOverlay(message);

                // Try to add overlay to the root page's layout
                if (rootPage is ContentPage contentPage)
                {
                    AddOverlayToContentPage(contentPage, _loadingOverlay);
                }
                else if (rootPage is Shell shell)
                {
                    // For Shell, try to add to the current page
                    var currentPage = shell.CurrentPage;
                    if (currentPage is ContentPage shellContentPage)
                    {
                        AddOverlayToContentPage(shellContentPage, _loadingOverlay);
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
            if (!_isLoading || _loadingOverlay == null)
                return;

            _isLoading = false;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Remove the overlay from its parent
                if (_loadingOverlay?.Parent is Grid parentGrid)
                {
                    parentGrid.Children.Remove(_loadingOverlay);
                    
                    // If the parent grid was our wrapper, unwrap it
                    if (parentGrid.Parent is ContentPage contentPage && 
                        parentGrid.Children.Count == 1 &&
                        parentGrid.StyleId == "LoadingWrapper")
                    {
                        var originalContent = parentGrid.Children[0];
                        parentGrid.Children.Clear();
                        contentPage.Content = (View)originalContent;
                    }
                }
                
                _loadingOverlay = null;
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

    private static Page? GetRootPage()
    {
        if (Application.Current?.Windows.Count > 0)
        {
            return Application.Current.Windows[0].Page;
        }
        return null;
    }

    private static void AddOverlayToContentPage(ContentPage contentPage, Grid overlay)
    {
        var existingContent = contentPage.Content;
        
        if (existingContent == null)
            return;

        // Check if already wrapped
        if (existingContent is Grid existingGrid && existingGrid.StyleId == "LoadingWrapper")
        {
            // Just add the overlay to existing wrapper
            existingGrid.Children.Add(overlay);
            return;
        }

        // Create a wrapper grid
        var wrapperGrid = new Grid
        {
            StyleId = "LoadingWrapper"
        };
        
        // Move existing content to wrapper
        contentPage.Content = null; // Detach first
        wrapperGrid.Children.Add(existingContent);
        wrapperGrid.Children.Add(overlay);
        
        contentPage.Content = wrapperGrid;
    }

    private static Grid CreateLoadingOverlay(string? message)
    {
        var overlay = new Grid
        {
            BackgroundColor = Color.FromArgb("#CC1e1e1e"),
            ZIndex = 1000,
            InputTransparent = false
        };

        // Add tap gesture to prevent interaction with underlying content
        overlay.GestureRecognizers.Add(new TapGestureRecognizer());

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
