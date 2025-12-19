using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Repositories;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class RegisterVM : BaseViewModel
    {
        private readonly IAuthenticationRepository _authenticationRepository;

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public ICommand RegisterCommand { get => new Command(async () => await RegisterAsync()); }
        public ICommand OpenLoginPageCommand { get => new Command(OpenLoginPage); }

        public RegisterVM(IAuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;
        }

        private async void OpenLoginPage()
        {
            await Shell.Current.GoToAsync("///LoginPage");
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || 
                string.IsNullOrWhiteSpace(LastName) || 
                string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Username) || 
                string.IsNullOrWhiteSpace(Password))
                return;

            await ExecuteAsync(async () =>
            {
                var registerModel = new RegisterModel
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    Email = this.Email,
                    Username = this.Username,
                    Password = this.Password
                };
                var response = await _authenticationRepository.Register(registerModel);
                await Shell.Current.GoToAsync("///LoginPage");
            }, "Creating your account...");
        }
    }
}
