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

        public StartupViewModel(ITokenService tokenService, IAuthenticationRepository authenticationRepository, IUsersRepository usersRepository)
        {
            _tokenService = tokenService;
            _authenticationRepository = authenticationRepository;
            _usersRepository = usersRepository;
            InitializeAsync();
        }

        private async void InitializeAsync()
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
                    await Shell.Current.GoToAsync("///MainPage");
                }
                else
                {
                    await Shell.Current.GoToAsync("///LoginPage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Startup error: {ex.Message}");
                LoadingMessage = "Connection failed. Redirecting...";
                await Task.Delay(1000);
                await Shell.Current.GoToAsync("///LoginPage");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
