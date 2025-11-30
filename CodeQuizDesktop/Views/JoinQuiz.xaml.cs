using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class JoinQuiz : ContentPage
{
	public JoinQuiz(JoinQuizVM joinQuizVM)
	{
		InitializeComponent();
		BindingContext = joinQuizVM;
	}
}