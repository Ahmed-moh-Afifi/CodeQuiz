using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class LoginVM : BaseViewModel
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly ITokenService _tokenService;
        private readonly INavigationService _navigationService;

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public ICommand LoginCommand { get => new Command(async () => await LoginAsync()); }
        public ICommand OpenRegisterPageCommand { get => new Command(async () => await OpenRegisterPageAsync()); }

        public LoginVM(IAuthenticationRepository authenticationRepository, ITokenService tokenService, INavigationService navigationService)
        {
            _authenticationRepository = authenticationRepository;
            _tokenService = tokenService;
            _navigationService = navigationService;
        }

        private async Task OpenHomePage()
        {
            await _navigationService.GoToAsync("///MainPage");
        }

        public async Task OpenRegisterPageAsync()
        {
            await _navigationService.GoToAsync("///RegisterPage");
        }

        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                return;

            await ExecuteAsync(async () =>
            {
                var loginModel = new LoginModel { Username = this.Username, Password = this.Password };
                var response = await _authenticationRepository.Login(loginModel);
                System.Diagnostics.Debug.WriteLine($"Username: {response.User.UserName}");
                await OpenHomePage();
            }, "Logging in...");
        }
    }
}
