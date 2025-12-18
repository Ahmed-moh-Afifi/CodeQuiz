using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class GradeAttempt : ContentPage
{
	public GradeAttempt(GradeAttemptVM gradeAttemptVM)
	{
		InitializeComponent();
		BindingContext = gradeAttemptVM;

    }
}