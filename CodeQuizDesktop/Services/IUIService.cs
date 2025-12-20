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
}
