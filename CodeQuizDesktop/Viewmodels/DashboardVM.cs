using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using System.Collections.ObjectModel;

namespace CodeQuizDesktop.Viewmodels
{
    public class DashboardVM : BaseViewModel
    {
        private ObservableCollection<ExamineeAttempt> joinedAttempts;
        public ObservableCollection<ExamineeAttempt> JoinedAttempts
        {
            get => joinedAttempts;
            set { joinedAttempts = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ExaminerQuiz> createdQuizzes;
        public ObservableCollection<ExaminerQuiz> CreatedQuizzes
        {
            get => createdQuizzes;
            set { createdQuizzes = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ExaminerQuiz> upcomingQuizzes = new();
        public ObservableCollection<ExaminerQuiz> UpcomingQuizzes
        {
            get => upcomingQuizzes;
            set { upcomingQuizzes = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ExaminerQuiz> endedQuizzes = new();
        public ObservableCollection<ExaminerQuiz> EndedQuizzes
        {
            get => endedQuizzes;
            set { endedQuizzes = value; OnPropertyChanged(); }
        }

        public DashboardVM()
        {
            JoinedAttempts = new ObservableCollection<ExamineeAttempt>();
            CreatedQuizzes = new ObservableCollection<ExaminerQuiz>();
        }
    }
}
