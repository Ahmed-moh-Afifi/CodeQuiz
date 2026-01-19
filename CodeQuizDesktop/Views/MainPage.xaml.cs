using CodeQuizDesktop.Logging;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Viewmodels;
using CodeQuizDesktop.Views;
using System.Windows.Input;

namespace CodeQuizDesktop
{
    public partial class MainPage : ContentPage
    {
        private readonly IAuthenticationRepository authenticationRepository;
        private readonly DashboardVM _dashboardVM;
        private readonly CreatedQuizzesVM _createdQuizzesVM;
        private readonly JoinedQuizzesVM _joinedQuizzesVM;
        private User user;
        private bool dashboardSelected = true;
        private bool createdSelected = false;
        private bool joinedSelected = false;

        public User User
        {
            get { return user; }
            set
            {
                user = value;
                OnPropertyChanged();
            }
        }

        public bool DashboardSelected
        {
            get { return dashboardSelected; }
            set
            {
                dashboardSelected = value;
                OnPropertyChanged();
                if (value)
                {
                    CreatedSelected = false;
                    JoinedSelected = false;
                }
            }
        }

        public bool CreatedSelected
        {
            get { return createdSelected; }
            set
            {
                createdSelected = value;
                OnPropertyChanged();
                if (value)
                {
                    DashboardSelected = false;
                    JoinedSelected = false;
                }
            }
        }

        public bool JoinedSelected
        {
            get { return joinedSelected; }
            set
            {
                joinedSelected = value;
                OnPropertyChanged();
                if (value)
                {
                    DashboardSelected = false;
                    CreatedSelected = false;
                }
            }
        }

        public ICommand LogoutCommand { get => new Command(Logout); }
        public ICommand OpenProfileCommand { get => new Command(async () => await Shell.Current.GoToAsync(nameof(ProfilePage))); }

        public MainPage(DashboardVM dashboardVM, CreatedQuizzesVM createdQuizzesVM, JoinedQuizzesVM joinedQuizzesVM, IUsersRepository usersRepository, IAuthenticationRepository authenticationRepository, IAppLogger<MainPage> logger)
        {
            InitializeComponent();
            _dashboardVM = dashboardVM;
            _createdQuizzesVM = createdQuizzesVM;
            _joinedQuizzesVM = joinedQuizzesVM;

            logger.LogInfo("Executing Constructor (creating viewmodels...)");
            BindingContext = this;
            Dashboard.BindingContext = dashboardVM;
            CreatedQuizzes.BindingContext = createdQuizzesVM;
            JoinedQuizzes.BindingContext = joinedQuizzesVM;
            this.authenticationRepository = authenticationRepository;
            User = authenticationRepository.LoggedInUser!;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _dashboardVM.InitializeAsync();
            await _createdQuizzesVM.InitializeAsync();
            await _joinedQuizzesVM.InitializeAsync();
        }

        public async void Logout()
        {
            await authenticationRepository.Logout();

            await Shell.Current.GoToAsync("///LoginPage");
        }
    }
}
