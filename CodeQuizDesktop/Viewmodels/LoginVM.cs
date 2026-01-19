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
        public ICommand ForgotPassCommand { get => new Command(async () => await ForgotPassAsync()); }

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

        public async Task ForgotPassAsync()
        {
            string? email = null;

            if (UIService != null)
            {
                email = await UIService.ShowInputAsync(
                    "Forgot Password",
                    "Enter your email address and we'll send you a link to reset your password.",
                    "email@example.com",
                    "Send Reset Link",
                    "Cancel",
                    Keyboard.Email,
                    "");
            }
            else if (Application.Current?.MainPage != null)
            {
                email = await Application.Current.MainPage.DisplayPromptAsync(
                    "Forgot Password",
                    "Enter your email address:",
                    "Send Reset Link",
                    "Cancel",
                    null,
                    -1,
                    Keyboard.Email,
                    "");
            }

            if (string.IsNullOrWhiteSpace(email))
                return;

            Exception? caughtException = null;

            try
            {
                if (UIService != null)
                    await UIService.ShowLoadingAsync("Sending reset link...");

                await _authenticationRepository.ForgotPassword(new ForgetPasswordModel { Email = email });
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
            finally
            {
                if (UIService != null)
                    await UIService.HideLoadingAsync();
            }

            // Show dialogs after loading is hidden to prevent overlay conflicts
            if (caughtException != null)
            {
                if (UIService != null)
                    await UIService.ShowErrorAsync(caughtException, "Failed to send reset link");
                else if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to send reset link: " + caughtException.Message, "OK");
            }
            else
            {
                if (UIService != null)
                    await UIService.ShowSuccessAsync("Email Sent", "If the email exists, a password reset link has been sent to your inbox.", "OK");
                else if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Success", "If the email exists, a password reset link has been sent.", "OK");
            }
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
