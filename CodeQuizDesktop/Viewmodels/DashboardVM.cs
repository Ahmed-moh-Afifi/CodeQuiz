using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class DashboardVM : BaseViewModel
    {
        private IAttemptsRepository _attemptsRepository;
        private IQuizzesRepository _quizzesRepository;

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

        protected ObservableCollection<ExamineeAttempt> initializedJoinedAttempts = new();
        protected ObservableCollection<ExaminerQuiz> initializedCreatedQuizzes = new();

        private void UpdateFilteredQuizzes()
        {
            upcomingQuizzes.Clear();
            endedQuizzes.Clear();

            foreach (var quiz in CreatedQuizzes)
            {
                if (quiz.StartDate > DateTime.Now ||
                    (quiz.StartDate <= DateTime.Now && quiz.EndDate > DateTime.Now))
                {
                    upcomingQuizzes.Add(quiz);
                }
                else
                {
                    endedQuizzes.Add(quiz);
                }
            }
        }

        public ICommand ContinueAttemptCommand => new Command<ExamineeAttempt>(OnContinueAttempt);
        public ICommand ViewResultsCommand => new Command<ExamineeAttempt>(OnViewResults);
        public ICommand ViewCreatedQuizCommand => new Command<ExaminerQuiz>(OnViewCreatedQuiz);
        public ICommand DeleteCreatedQuizCommand => new Command<ExaminerQuiz>(OnDeleteCreatedQuiz);

        private async void OnContinueAttempt(ExamineeAttempt attempt) { }
        private async void OnViewResults(ExamineeAttempt attempt) { }
        private async void OnViewCreatedQuiz(ExaminerQuiz quiz) { }
        private async void OnDeleteCreatedQuiz(ExaminerQuiz quiz) { }

        public DashboardVM(
            IAttemptsRepository attemptsRepository,
            IQuizzesRepository quizzesRepository)
        {
            _attemptsRepository = attemptsRepository;
            _quizzesRepository = quizzesRepository;

            JoinedAttempts = initializedJoinedAttempts;
            CreatedQuizzes = initializedCreatedQuizzes;
        }
    }
}
