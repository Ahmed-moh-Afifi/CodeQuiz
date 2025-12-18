using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class ExamineeReviewQuiz : ContentPage
{
	public ExamineeReviewQuiz(ExamineeReviewQuizVM examineeReviewQuizVM)
	{
		InitializeComponent();
		BindingContext = examineeReviewQuizVM;
	}
}