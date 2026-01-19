using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
