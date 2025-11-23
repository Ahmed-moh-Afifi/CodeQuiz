using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class Login : ContentPage
{
	public Login(LoginVM loginVM)
	{
		InitializeComponent();
		BindingContext = loginVM;

    }
}