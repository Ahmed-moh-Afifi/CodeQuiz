using CodeQuizDesktop.Views;

namespace CodeQuizDesktop.Services;

/// <summary>
/// Service for handling UI operations including alerts and loading indicators.
/// </summary>
public interface IUIService : IAlertService
{
    /// <summary>
    /// Shows a loading overlay with an optional message.
    /// </summary>
    /// <param name="message">Optional message to display with the loading indicator.</param>
    Task ShowLoadingAsync(string? message = null);

    /// <summary>
    /// Hides the loading overlay.
    /// </summary>
    Task HideLoadingAsync();

    /// <summary>
    /// Gets whether the loading overlay is currently visible.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Executes an async operation while showing a loading indicator.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="loadingMessage">Optional message to display during loading.</param>
    /// <returns>The result of the operation.</returns>
    Task<T> ExecuteWithLoadingAsync<T>(Func<Task<T>> operation, string? loadingMessage = null);

    /// <summary>
    /// Executes an async operation while showing a loading indicator.
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="loadingMessage">Optional message to display during loading.</param>
    Task ExecuteWithLoadingAsync(Func<Task> operation, string? loadingMessage = null);

    /// <summary>
    /// Shows a confirmation dialog styled for destructive actions (delete, remove, etc.)
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The confirmation message.</param>
    /// <param name="accept">The accept button text.</param>
    /// <param name="cancel">The cancel button text.</param>
    /// <returns>True if user confirmed, false otherwise.</returns>
    Task<bool> ShowDestructiveConfirmationAsync(string title, string message, string accept = "Delete", string cancel = "Cancel");

    /// <summary>
    /// Shows a success dialog with a checkmark icon.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The success message.</param>
    /// <param name="okText">The OK button text.</param>
    Task ShowSuccessAsync(string title, string message, string okText = "OK");

    /// <summary>
    /// Shows an input dialog that allows the user to enter text.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The prompt message.</param>
    /// <param name="placeholder">Placeholder text for the input field.</param>
    /// <param name="submitText">The submit button text.</param>
    /// <param name="cancelText">The cancel button text.</param>
    /// <param name="keyboard">The keyboard type for the input.</param>
    /// <param name="initialValue">Initial value for the input field.</param>
    /// <returns>The entered text, or null if cancelled.</returns>
    Task<string?> ShowInputAsync(
        string title,
        string message,
        string placeholder = "",
        string submitText = "Submit",
        string cancelText = "Cancel",
        Keyboard? keyboard = null,
        string initialValue = "");

    /// <summary>
    /// Shows a dialog displaying a quiz code with options to copy and share.
    /// </summary>
    /// <param name="code">The quiz code to display.</param>
    /// <param name="title">Optional title for the dialog.</param>
    /// <param name="subtitle">Optional subtitle for the dialog.</param>
    /// <param name="showShareButton">Whether to show the share button.</param>
    /// <returns>The result of the dialog interaction.</returns>
    Task<QuizCodeDialogResult> ShowQuizCodeAsync(
        string code,
        string? title = null,
        string? subtitle = null,
        bool showShareButton = true);

    /// <summary>
    /// Invokes an action on the main thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void BeginInvokeOnMainThread(Action action);
}
