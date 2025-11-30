using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class ExaminerViewQuiz : ContentPage
{
	public ExaminerViewQuiz(ExaminerViewQuizVM examinerViewQuizVM)
	{
		InitializeComponent();
		BindingContext = examinerViewQuizVM;
	}
}