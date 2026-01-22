using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Logging;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Views;
using System.Text.Json;

namespace CodeQuizDesktop.Services;

/// <summary>
/// Service for handling UI operations including alerts and loading indicators.
/// Uses custom themed dialogs instead of default MAUI alerts.
/// </summary>
public class UIService(IAppLogger<UIService> logger) : IUIService
{
    private bool _isLoading;
    private Grid? _loadingOverlay;
    private readonly SemaphoreSlim _loadingSemaphore = new(1, 1);
    private readonly SemaphoreSlim _dialogSemaphore = new(1, 1);

    public bool IsLoading => _isLoading;

    #region Alert Methods

    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        await _dialogSemaphore.WaitAsync();
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var rootPage = GetRootPage();
                if (rootPage == null) return;

                var dialog = new ErrorDialog();
                var dialogTask = dialog.ShowAsync(title, message, cancel);

                // Add dialog to page
                AddDialogToPage(rootPage, dialog);

                await dialogTask;

                // Remove dialog from page
                RemoveDialogFromPage(rootPage, dialog);
            });
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        await _dialogSemaphore.WaitAsync();
        try
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var rootPage = GetRootPage();
                if (rootPage == null) return false;

                var dialog = new ConfirmationDialog();
                var dialogTask = dialog.ShowAsync(title, message, accept, cancel, false);

                // Add dialog to page
                AddDialogToPage(rootPage, dialog);

                var result = await dialogTask;

                // Remove dialog from page
                RemoveDialogFromPage(rootPage, dialog);

                return result;
            });
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }

    /// <summary>
    /// Shows a confirmation dialog styled for destructive actions (delete, remove, etc.)
    /// </summary>
    public async Task<bool> ShowDestructiveConfirmationAsync(string title, string message, string accept = "Delete", string cancel = "Cancel")
    {
        await _dialogSemaphore.WaitAsync();
        try
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var rootPage = GetRootPage();
                if (rootPage == null) return false;

                var dialog = new ConfirmationDialog();
                var dialogTask = dialog.ShowAsync(title, message, accept, cancel, isDestructive: true);

                // Add dialog to page
                AddDialogToPage(rootPage, dialog);

                var result = await dialogTask;

                // Remove dialog from page
                RemoveDialogFromPage(rootPage, dialog);

                return result;
            });
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }

    /// <summary>
    /// Shows a success dialog with a checkmark icon.
    /// </summary>
    public async Task ShowSuccessAsync(string title, string message, string okText = "OK")
    {
        await _dialogSemaphore.WaitAsync();
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var rootPage = GetRootPage();
                if (rootPage == null) return;

                var dialog = new SuccessDialog();
                var dialogTask = dialog.ShowAsync(title, message, okText);

                // Add dialog to page
                AddDialogToPage(rootPage, dialog);

                await dialogTask;

                // Remove dialog from page
                RemoveDialogFromPage(rootPage, dialog);
            });
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }

    /// <summary>
    /// Shows an input dialog that allows the user to enter text.
    /// </summary>
    public async Task<string?> ShowInputAsync(
        string title,
        string message,
        string placeholder = "",
        string submitText = "Submit",
        string cancelText = "Cancel",
        Keyboard? keyboard = null,
        string initialValue = "")
    {
        await _dialogSemaphore.WaitAsync();
        try
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var rootPage = GetRootPage();
                if (rootPage == null) return null;

                var dialog = new InputDialog();
                var dialogTask = dialog.ShowAsync(title, message, placeholder, submitText, cancelText, keyboard, initialValue);

                // Add dialog to page
                AddDialogToPage(rootPage, dialog);

                var result = await dialogTask;

                // Remove dialog from page
                RemoveDialogFromPage(rootPage, dialog);

                return result;
            });
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }

    /// <summary>
    /// Shows a dialog displaying a quiz code with options to copy and share.
    /// </summary>
    public async Task<QuizCodeDialogResult> ShowQuizCodeAsync(
        string code,
        string? title = null,
        string? subtitle = null,
        bool showShareButton = true)
    {
        await _dialogSemaphore.WaitAsync();
        try
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var rootPage = GetRootPage();
                if (rootPage == null) return QuizCodeDialogResult.Cancelled;

                var dialog = new QuizCodeDialog();
                var dialogTask = dialog.ShowAsync(code, title, subtitle, showShareButton);

                // Add dialog to page
                AddDialogToPage(rootPage, dialog);

                var result = await dialogTask;

                // Remove dialog from page
                RemoveDialogFromPage(rootPage, dialog);

                return result;
            });
        }
        finally
        {
            _dialogSemaphore.Release();
        }
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

    private static void AddDialogToPage(Page rootPage, ContentView dialog)
    {
        if (rootPage is ContentPage contentPage)
        {
            AddDialogToContentPage(contentPage, dialog);
        }
        else if (rootPage is Shell shell)
        {
            var currentPage = shell.CurrentPage;
            if (currentPage is ContentPage shellContentPage)
            {
                AddDialogToContentPage(shellContentPage, dialog);
            }
        }
    }

    private static void AddDialogToContentPage(ContentPage contentPage, ContentView dialog)
    {
        var existingContent = contentPage.Content;

        if (existingContent == null)
            return;

        // Check if root is a Grid (either our wrapper or the user's layout)
        // reusing it prevents reparenting controls like WebView which reload when moved
        if (existingContent is Grid existingGrid)
        {
            // Ensure dialog spans all rows/columns to cover everything
            int rowSpan = existingGrid.RowDefinitions.Count > 0 ? existingGrid.RowDefinitions.Count : 1;
            int colSpan = existingGrid.ColumnDefinitions.Count > 0 ? existingGrid.ColumnDefinitions.Count : 1;

            Grid.SetRowSpan(dialog, rowSpan);
            Grid.SetColumnSpan(dialog, colSpan);

            // Add with z-index implies on top (last child)
            existingGrid.Children.Add(dialog);
            return;
        }

        // Create a wrapper grid
        var wrapperGrid = new Grid
        {
            StyleId = "DialogWrapper"
        };

        // Move existing content to wrapper
        contentPage.Content = null; // Detach first
        wrapperGrid.Children.Add(existingContent);
        wrapperGrid.Children.Add(dialog);

        contentPage.Content = wrapperGrid;
    }

    private static void RemoveDialogFromPage(Page rootPage, ContentView dialog)
    {
        if (rootPage is ContentPage contentPage)
        {
            RemoveDialogFromContentPage(contentPage, dialog);
        }
        else if (rootPage is Shell shell)
        {
            var currentPage = shell.CurrentPage;
            if (currentPage is ContentPage shellContentPage)
            {
                RemoveDialogFromContentPage(shellContentPage, dialog);
            }
        }
    }

    private static void RemoveDialogFromContentPage(ContentPage contentPage, ContentView dialog)
    {
        if (dialog.Parent is Grid parentGrid)
        {
            parentGrid.Children.Remove(dialog);

            // If the parent grid was our wrapper and only has original content, unwrap it
            if (parentGrid.Parent is ContentPage cp &&
                parentGrid.Children.Count == 1 &&
                parentGrid.StyleId == "DialogWrapper")
            {
                var originalContent = parentGrid.Children[0];
                parentGrid.Children.Clear();
                cp.Content = (View)originalContent;
            }
        }
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

        // Check if root is a Grid (either our wrapper or the user's layout)
        // reusing it prevents reparenting controls like WebView which reload when moved
        if (existingContent is Grid existingGrid)
        {
            // Ensure overlay spans all rows/columns to cover everything
            int rowSpan = existingGrid.RowDefinitions.Count > 0 ? existingGrid.RowDefinitions.Count : 1;
            int colSpan = existingGrid.ColumnDefinitions.Count > 0 ? existingGrid.ColumnDefinitions.Count : 1;

            Grid.SetRowSpan(overlay, rowSpan);
            Grid.SetColumnSpan(overlay, colSpan);

            // Add with z-index implies on top (last child)
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

    public void BeginInvokeOnMainThread(Action action)
    {
        MainThread.BeginInvokeOnMainThread(action);
    }
}
