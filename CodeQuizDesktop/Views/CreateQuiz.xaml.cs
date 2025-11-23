using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class CreateQuiz : ContentPage
{
	public CreateQuiz(CreateQuizVM createQuizVM)
	{
		InitializeComponent();
		BindingContext = createQuizVM;
	}
}