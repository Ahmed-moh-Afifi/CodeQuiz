using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class Dashboard : ContentView
{
    public Dashboard()
    {
        InitializeComponent();

        BindingContext = MauiProgram.GetService<DashboardVM>();
    }
}
