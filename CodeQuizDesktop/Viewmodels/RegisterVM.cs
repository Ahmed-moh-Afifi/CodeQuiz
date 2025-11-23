using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class RegisterVM : BaseViewModel
    {
        public ICommand RegisterCommand { get => new Command(OpenHomePage); }

        private async void OpenHomePage()
        {
            await Shell.Current.GoToAsync("///MainPage");
        }

        public ICommand OpenLoginPageCommand { get => new Command(OpenLoginPage); }

        private async void OpenLoginPage()
        {
            await Shell.Current.GoToAsync("///LoginPage");
        }
        public RegisterVM() 
        {

        }
    }
}
