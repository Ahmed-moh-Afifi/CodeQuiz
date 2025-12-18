using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class EditQuiz : ContentPage
{
	public EditQuiz(EditQuizVM editQuizVM)
	{
		InitializeComponent();
		BindingContext = editQuizVM;
	}
}