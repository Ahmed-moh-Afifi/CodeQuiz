namespace CodeQuizDesktop.Services;

public interface IAlertService
{
    Task ShowAlertAsync(string title, string message, string cancel = "OK");
    Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No");
    Task ShowErrorAsync(string message, string? title = null);
    Task ShowErrorAsync(Exception exception, string? userFriendlyMessage = null);
}
