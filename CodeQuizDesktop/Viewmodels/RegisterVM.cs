using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class RegisterVM : BaseViewModel
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly INavigationService _navigationService;

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public ICommand RegisterCommand { get => new Command(async () => await RegisterAsync()); }
        public ICommand OpenLoginPageCommand { get => new Command(async () => await OpenLoginPageAsync()); }

        public RegisterVM(IAuthenticationRepository authenticationRepository, INavigationService navigationService)
        {
            _authenticationRepository = authenticationRepository;
            _navigationService = navigationService;
        }

        public async Task OpenLoginPageAsync()
        {
            await _navigationService.GoToAsync("///LoginPage");
        }

        public async Task RegisterAsync()
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
                await _navigationService.GoToAsync("///LoginPage");
            }, "Creating your account...");
        }
    }
}
