using CommunityToolkit.Maui.Views;
using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class QuizSettingsDialog : Popup<string?>
{
	public QuizSettingsDialog(QuizSettingsDialogVM quizSettingsDialogVM)
	{
		InitializeComponent();
        BindingContext = quizSettingsDialogVM;
	}

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }
}