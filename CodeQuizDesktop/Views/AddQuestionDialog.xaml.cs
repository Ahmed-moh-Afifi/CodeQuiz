using CommunityToolkit.Maui.Views;
using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class AddQuestionDialog : Popup<string?>
{
	public AddQuestionDialog(AddQuestionDialogVM addQuestionDialogVM)
	{
		InitializeComponent();
		BindingContext = addQuestionDialogVM;
	}

	private async void OnAddButtonClicked(object sender, EventArgs e)
	{
		await CloseAsync(null);
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
	{
		await CloseAsync(null);
    }
}