using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class LoginVM : BaseViewModel
    {
        public ICommand LoginCommand { get => new Command(OpenHomePage); }

        private async void OpenHomePage()
        {
            //System.Diagnostics.Debug.WriteLine("Openning create quiz page...");
            //await Shell.Current.GoToAsync("///CreateQuizPage");
            await Shell.Current.GoToAsync("///MainPage");

        }

        public ICommand OpenRegisterPageCommand { get => new Command(OpenRegisterPage); }

        private async void OpenRegisterPage()
        {
            await Shell.Current.GoToAsync("///RegisterPage");
        }
        public LoginVM()
        {
        }
    }
}
