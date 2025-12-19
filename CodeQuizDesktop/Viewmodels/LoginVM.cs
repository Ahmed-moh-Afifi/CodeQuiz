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

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        
        public ICommand LoginCommand { get => new Command(async () => await LoginAsync()); }
        public ICommand OpenRegisterPageCommand { get => new Command(OpenRegisterPage); }

        public LoginVM(IAuthenticationRepository authenticationRepository, ITokenService tokenService)
        {
            _authenticationRepository = authenticationRepository;
            _tokenService = tokenService;
        }

        private async Task OpenHomePage()
        {
            await Shell.Current.GoToAsync("///MainPage");
        }

        private async void OpenRegisterPage()
        {
            await Shell.Current.GoToAsync("///RegisterPage");
        }

        private async Task LoginAsync()
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
