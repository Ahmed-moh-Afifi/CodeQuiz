using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop
{
    public partial class MainPage : ContentPage
    {
        private string userFirstName;

        public string UserFirstName
        {
            get { return userFirstName; }
            set 
            { 
                userFirstName = value;
                OnPropertyChanged();
            }
        }


        private bool dashboardSelected = true;
        private bool createdSelected = false;
        private bool joinedSelected = false;

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




        public MainPage(DashboardVM dashboardVM, CreatedQuizzesVM createdQuizzesVM, JoinedQuizzesVM joinedQuizzesVM, IUsersRepository usersRepository)
        {
            InitializeComponent();
            BindingContext = this;
            Dashboard.BindingContext = dashboardVM;
            CreatedQuizzes.BindingContext = createdQuizzesVM;
            JoinedQuizzes.BindingContext = joinedQuizzesVM;

        }


    }

}
