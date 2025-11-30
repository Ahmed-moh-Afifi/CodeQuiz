using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class Startup : ContentPage
{
    public Startup(StartupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}