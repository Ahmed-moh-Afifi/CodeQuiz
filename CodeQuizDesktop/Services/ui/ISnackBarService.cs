namespace CodeQuizDesktop.Services.UI;

public interface ISnackBarService
{
    Task ShowAsync(string message, SnackBarType type = SnackBarType.Info);
}
 