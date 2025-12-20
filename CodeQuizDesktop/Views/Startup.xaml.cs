using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class Startup : ContentPage
{
    public Startup(StartupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StartupViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}