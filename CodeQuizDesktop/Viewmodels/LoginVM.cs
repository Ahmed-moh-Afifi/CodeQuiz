using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class LoginVM(IAuthenticationRepository authenticationRepository, ITokenService tokenService) : BaseViewModel
    {

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public ICommand LoginCommand { get => new Command(Login); }

        private async Task OpenHomePage()
        {
            await Shell.Current.GoToAsync("///MainPage");
        }

        public ICommand OpenRegisterPageCommand { get => new Command(OpenRegisterPage); }

        private async void OpenRegisterPage()
        {
            await Shell.Current.GoToAsync("///RegisterPage");
        }

        private async void Login()
        {
            if (Username == "" || Password == "")
                return;

            var loginModel = new LoginModel() { Username = this.Username, Password = this.Password };
            var response = await authenticationRepository.Login(loginModel);
            await tokenService.SaveTokens(response.TokenModel);
            System.Diagnostics.Debug.WriteLine($"Username: {response.User.UserName}");
            await OpenHomePage();
        }
    }
}
