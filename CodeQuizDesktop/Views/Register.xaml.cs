using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class Register : ContentPage
{
	public Register(RegisterVM registerVM)
	{
		InitializeComponent();
		BindingContext = registerVM;
    }
}