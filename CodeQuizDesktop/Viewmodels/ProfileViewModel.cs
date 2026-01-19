using CodeQuizDesktop.Models;
using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IAuthenticationRepository _authRepository;
        private readonly INavigationService _navigationService;

        private User? _user;
        public User? User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged();
            }
        }

        private string _oldPassword = "";
        public string OldPassword
        {
            get => _oldPassword;
            set
            {
                _oldPassword = value;
                OnPropertyChanged();
            }
        }

        private string _newPassword = "";
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged();
            }
        }

        private string _confirmPassword = "";
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadUserCommand => new Command(async () => await LoadUser());
        public ICommand ResetPasswordCommand => new Command(async () => await ResetPassword());
        public ICommand GoBackCommand => new Command(async () => await _navigationService.GoToAsync(".."));

        public ProfileViewModel(IUsersRepository usersRepository, IAuthenticationRepository authRepository, INavigationService navigationService)
        {
            _usersRepository = usersRepository;
            _authRepository = authRepository;
            _navigationService = navigationService;

            // Try to get cached user from auth repository
            User = _authRepository.LoggedInUser;
        }

        public async Task LoadUser()
        {
            try
            {
                await ExecuteAsync(async () =>
                {
                    User = await _usersRepository.GetUser();
                }, "Loading Profile...");
            }
            catch (Exception ex)
            {
                if (UIService != null)
                    await UIService.ShowErrorAsync(ex, "Failed to load profile.");
            }
        }

        private async Task ResetPassword()
        {
            if (User == null) return;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(OldPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                if (UIService != null)
                    await UIService.ShowAlertAsync("Validation Error", "Please fill in all password fields.", "OK");
                return;
            }

            if (NewPassword.Length < 6)
            {
                if (UIService != null)
                    await UIService.ShowAlertAsync("Validation Error", "New password must be at least 6 characters long.", "OK");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                if (UIService != null)
                    await UIService.ShowAlertAsync("Validation Error", "New passwords do not match.", "OK");
                return;
            }

            // Show confirmation dialog
            if (UIService != null)
            {
                var confirmed = await UIService.ShowConfirmationAsync(
                    "Confirm Password Change",
                    "Are you sure you want to change your password?",
                    "Yes, Change Password",
                    "Cancel");

                if (!confirmed) return;
            }

            try
            {
                await ExecuteAsync(async () =>
                {
                    var model = new ResetPasswordModel
                    {
                        Username = User.UserName,
                        Password = OldPassword,
                        NewPassword = NewPassword
                    };

                    await _authRepository.ResetPassword(model);

                    // Clear fields on success
                    OldPassword = "";
                    NewPassword = "";
                    ConfirmPassword = "";
                }, "Updating Password...");

                // Show success message after loading is hidden
                if (UIService != null)
                    await UIService.ShowSuccessAsync("Success", "Your password has been changed successfully.", "OK");
            }
            catch (Exception ex)
            {
                if (UIService != null)
                    await UIService.ShowErrorAsync(ex, "Failed to reset password. Please check your current password and try again.");
            }
        }
    }
}
