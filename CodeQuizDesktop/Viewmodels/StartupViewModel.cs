using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Viewmodels
{
    public class StartupViewModel : BaseViewModel
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IUsersRepository _usersRepository;

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string _loadingMessage = "Checking authentication...";
        public string LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                _loadingMessage = value;
                OnPropertyChanged();
            }
        }

        private readonly INavigationService _navigationService;

        public StartupViewModel(ITokenService tokenService, IAuthenticationRepository authenticationRepository, IUsersRepository usersRepository, INavigationService navigationService)
        {
            _tokenService = tokenService;
            _authenticationRepository = authenticationRepository;
            _usersRepository = usersRepository;
            _navigationService = navigationService;
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Starting up...";

                await Task.Delay(1000); // Brief delay for splash visibility

                LoadingMessage = "Checking authentication...";
                var token = await _tokenService.GetValidTokens();

                if (token != null)
                {
                    LoadingMessage = "Loading user profile...";
                    _authenticationRepository.LoggedInUser = await _usersRepository.GetUser();
                    await _navigationService.GoToAsync("///MainPage");
                }
                else
                {
                    await _navigationService.GoToAsync("///LoginPage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Startup error: {ex.Message}");
                LoadingMessage = "Connection failed. Redirecting...";
                await Task.Delay(1000);
                await _navigationService.GoToAsync("///LoginPage");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
