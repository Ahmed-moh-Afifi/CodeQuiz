using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class RegisterVM(IAuthenticationRepository authenticationRepository) : BaseViewModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = ""; 
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";  
        public ICommand RegisterCommand { get => new Command(Register); }

        public ICommand OpenLoginPageCommand { get => new Command(OpenLoginPage); }

        private async void OpenLoginPage()
        {
            await Shell.Current.GoToAsync("///LoginPage");
        }

        private async void Register()
        {
            if (FirstName == "" || LastName == "" || Email == "" || Username == "" || Password == "")
                return;

            var registerModel = new RegisterModel()
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Username = this.Username,
                Password = this.Password
            };
            var response = await authenticationRepository.Register(registerModel);
            await Shell.Current.GoToAsync("///LoginPage");

        }

    }
}
