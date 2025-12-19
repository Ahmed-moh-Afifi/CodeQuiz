using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui.Core.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class JoinedQuizzesVM : BaseViewModel
    {
        private readonly IAttemptsRepository _attemptsRepository;

        private ObservableCollection<ExamineeAttempt> allExamineeAttempts = [];

        public ObservableCollection<ExamineeAttempt> AllExamineeAttempts
        {
            get { return allExamineeAttempts; }
            set
            {
                allExamineeAttempts = value;
                OnPropertyChanged();
            }
        }

        public string QuizCode { get; set; } = "";
        public ICommand JoinQuizCommand { get => new Command(async () => await JoinQuizAsync()); }
        public ICommand ContinueAttemptCommand { get => new Command<ExamineeAttempt>(async (a) => await OnContinueAttemptAsync(a)); }
        public ICommand ReviewAttemptCommand { get => new Command<ExamineeAttempt>(OnReviewAttempt); }

        private async Task JoinQuizAsync()
        {
            if (string.IsNullOrWhiteSpace(QuizCode))
                return;

            await ExecuteAsync(async () =>
            {
                var beginAttemptResponse = new BeginAttemptRequest { QuizCode = this.QuizCode };
                var response = await _attemptsRepository.BeginAttempt(beginAttemptResponse);
                await Shell.Current.GoToAsync(nameof(JoinQuiz), new Dictionary<string, object> { { "attempt", response! } });
            }, "Joining quiz...");
        }

        private async Task OnContinueAttemptAsync(ExamineeAttempt examineeAttempt)
        {
            System.Diagnostics.Debug.WriteLine($"Clicked: {examineeAttempt.Quiz.Code}");
            await ExecuteAsync(async () =>
            {
                var beginAttemptResponse = new BeginAttemptRequest { QuizCode = examineeAttempt.Quiz.Code };
                var response = await _attemptsRepository.BeginAttempt(beginAttemptResponse);
                await Shell.Current.GoToAsync(nameof(JoinQuiz), new Dictionary<string, object> { { "attempt", response! } });
            }, "Loading quiz...");
        }

        private async void OnReviewAttempt(ExamineeAttempt examineeAttempt)
        {
            await Shell.Current.GoToAsync(nameof(ExamineeReviewQuiz), new Dictionary<string, object>
            {
                { "attempt", examineeAttempt! }
            });
        }

        private async void InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                var response = await _attemptsRepository.GetUserAttempts();
                AllExamineeAttempts = response.ToObservableCollection();
            }, "Loading quizzes...");
        }

        public JoinedQuizzesVM(IAttemptsRepository attemptsRepository)
        {
            _attemptsRepository = attemptsRepository;
            InitializeAsync();
            _attemptsRepository.SubscribeCreate(a =>
            {
                if (AllExamineeAttempts.FirstOrDefault(at => at.Id == a.Id) == null)
                    AllExamineeAttempts.Add(a);
            });
            _attemptsRepository.SubscribeUpdate(a =>
            {
                var element = AllExamineeAttempts.First(at => at.Id == a.Id);
                var idx = AllExamineeAttempts.IndexOf(element);
                AllExamineeAttempts.Remove(element);
                AllExamineeAttempts.Insert(idx, a);
            });
        }
    }
}
