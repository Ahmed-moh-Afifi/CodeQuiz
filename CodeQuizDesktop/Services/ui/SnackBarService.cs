using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Graphics;

namespace CodeQuizDesktop.Services.UI;

public class SnackBarService : ISnackBarService
{
    public async Task ShowAsync(string message, SnackBarType type = SnackBarType.Info)
    {
        var (bg, text) = GetColors(type);

        var options = new SnackbarOptions
        {
            BackgroundColor = bg,
            TextColor = text,
            ActionButtonTextColor = text,
            CornerRadius = new CornerRadius(8),
            CharacterSpacing = 0
        };

        var snackBar = Snackbar.Make(
            message,
            visualOptions: options
        );

        await snackBar.Show();
    }

    private static (Color bg, Color text) GetColors(SnackBarType type)
    {
        return type switch
        {
            SnackBarType.Success => (Colors.DarkGreen, Colors.White),
            SnackBarType.Warning => (Colors.Goldenrod, Colors.Black),
            SnackBarType.Error   => (Colors.DarkRed, Colors.White),
            _                    => (Colors.DimGray, Colors.White)
        };
    }
}
