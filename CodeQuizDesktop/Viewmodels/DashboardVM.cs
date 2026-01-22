using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CommunityToolkit.Maui.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class DashboardVM : BaseViewModel
    {
        private IAttemptsRepository _attemptsRepository;
        private IQuizzesRepository _quizzesRepository;
        private readonly INavigationService _navigationService;
        private readonly IUIService _uiService;

        private ObservableCollection<ExamineeAttempt> joinedAttempts;
        public ObservableCollection<ExamineeAttempt> JoinedAttempts
        {
            get => joinedAttempts;
            set
            {
                // Unsubscribe from old collection
                if (joinedAttempts != null)
                {
                    joinedAttempts.CollectionChanged -= OnJoinedAttemptsChanged;
                }

                joinedAttempts = value;

                // Subscribe to new collection
                if (joinedAttempts != null)
                {
                    joinedAttempts.CollectionChanged += OnJoinedAttemptsChanged;
                }

                OnPropertyChanged();
                UpdateHasRunningAttempts();
            }
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

        private ObservableCollection<ExamineeAttempt> initializedJoinedAttempts = new();
        private ObservableCollection<ExaminerQuiz> initializedCreatedQuizzes = new();

        // Reactive property for hero section - updates in real-time
        private bool hasRunningAttempts;
        public bool HasRunningAttempts
        {
            get => hasRunningAttempts;
            private set
            {
                if (hasRunningAttempts != value)
                {
                    hasRunningAttempts = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnJoinedAttemptsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateHasRunningAttempts();
        }

        private void UpdateHasRunningAttempts()
        {
            HasRunningAttempts = JoinedAttempts?.Any(a => a.IsRunning) ?? false;
        }

        public ICommand ContinueAttemptCommand { get => new Command<ExamineeAttempt>(async (a) => await ContinueAttemptAsync(a)); }
        public ICommand ViewResultsCommand { get => new Command<ExamineeAttempt>(async (a) => await ViewResultsAsync(a)); }

        public ICommand ViewCreatedQuizCommand { get => new Command<ExaminerQuiz>(async (q) => await ViewCreatedQuizAsync(q)); }
        public ICommand DeleteCreatedQuizCommand { get => new Command<ExaminerQuiz>(async (q) => await DeleteCreatedQuizAsync(q)); }
        public ICommand ShowQuizCodeCommand { get => new Command<ExaminerQuiz>(async (q) => await ShowQuizCodeAsync(q)); }

        private async Task ShowQuizCodeAsync(ExaminerQuiz quiz)
        {
            await _uiService.ShowQuizCodeAsync(
                quiz.Code,
                "Quiz Code",
                $"Share this code to let participants join \"{quiz.Title}\"");
        }

        // Navigation commands for "View All" and "Manage All" links
        public ICommand ViewAllJoinedCommand { get => new Command(ViewAllJoined); }
        public ICommand ViewAllCreatedCommand { get => new Command(ViewAllCreated); }

        // Command to continue the first running attempt from the hero section
        public ICommand ContinueFirstRunningCommand { get => new Command(async () => await ContinueFirstRunningAsync()); }

        private async Task ContinueFirstRunningAsync()
        {
            var firstRunning = JoinedAttempts?.FirstOrDefault(a => a.IsRunning);
            if (firstRunning != null)
            {
                await ContinueAttemptAsync(firstRunning);
            }
        }

        private void ViewAllJoined()
        {
            // Navigate to Joined Quizzes tab by triggering the MainPage selection change
            if (Application.Current?.MainPage is Shell shell)
            {
                var mainPage = shell.CurrentPage as MainPage;
                if (mainPage != null)
                {
                    mainPage.JoinedSelected = true;
                }
            }
        }

        private void ViewAllCreated()
        {
            // Navigate to Created Quizzes tab by triggering the MainPage selection change
            if (Application.Current?.MainPage is Shell shell)
            {
                var mainPage = shell.CurrentPage as MainPage;
                if (mainPage != null)
                {
                    mainPage.CreatedSelected = true;
                }
            }
        }

        public async Task ContinueAttemptAsync(ExamineeAttempt attempt)
        {
            await ExecuteAsync(async () =>
            {
                var beginAttemptRequest = new BeginAttemptRequest() { QuizCode = attempt.Quiz.Code };
                var response = await _attemptsRepository.BeginAttempt(beginAttemptRequest);
                await _navigationService.GoToAsync($"///JoinQuizPage", new Dictionary<string, object> { { "attempt", response! } });
            }, "Loading quiz...");
        }

        public async Task ViewResultsAsync(ExamineeAttempt attempt)
        {
            // Navigate to the examinee review page using the registered route name
            await _navigationService.GoToAsync(nameof(CodeQuizDesktop.Views.ExamineeReviewQuiz), new Dictionary<string, object> { { "attempt", attempt } });
        }

        public async Task ViewCreatedQuizAsync(ExaminerQuiz quiz)
        {
            await _navigationService.GoToAsync(nameof(CodeQuizDesktop.Views.ExaminerViewQuiz), new Dictionary<string, object> { { "quiz", quiz } });
        }

        // Edit handler removed — editing is available in the full CreatedQuizzes view.

        public async Task DeleteCreatedQuizAsync(ExaminerQuiz quiz)
        {
            var confirm = await _uiService.ShowDestructiveConfirmationAsync(
                "Delete Quiz",
                $"Are you sure you want to delete \"{quiz.Title}\"? This action cannot be undone and will remove all associated attempts and grades.",
                "Delete",
                "Cancel");

            if (!confirm) return;

            await ExecuteAsync(async () =>
            {
                await _quizzesRepository.DeleteQuiz(quiz.Id);

                // Optimistically remove from local collections in case notifications don't arrive
                _uiService.BeginInvokeOnMainThread(() =>
                {
                    var local = CreatedQuizzes.FirstOrDefault(q => q.Id == quiz.Id);
                    if (local != null)
                    {
                        CreatedQuizzes.Remove(local);
                    }

                    var relatedAttempts = JoinedAttempts.Where(at => at.QuizId == quiz.Id).ToList();
                    foreach (var ra in relatedAttempts)
                        JoinedAttempts.Remove(ra);

                    UpdateFilteredQuizzes();
                });
            }, "Deleting quiz...");
        }

        private void UpdateFilteredQuizzes()
        {
            upcomingQuizzes.Clear();
            endedQuizzes.Clear();
            foreach (var quiz in CreatedQuizzes)
            {
                // Upcoming: Either not started yet OR currently running
                if (quiz.StartDate > DateTime.Now || (quiz.StartDate <= DateTime.Now && quiz.EndDate > DateTime.Now))
                {
                    upcomingQuizzes.Add(quiz);
                }
                else if (quiz.EndDate <= DateTime.Now)
                {
                    endedQuizzes.Add(quiz);
                }
            }
        }

        public async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                var attempts = await _attemptsRepository.GetUserAttempts();
                initializedJoinedAttempts.Clear();
                foreach (var attempt in attempts)
                {
                    // Compute GradePercentage locally if server didn't provide it but all solution grades are present
                    if (attempt.GradePercentage == null && attempt.Solutions != null && attempt.Quiz != null && attempt.Quiz.TotalPoints > 0)
                    {
                        if (attempt.Solutions.All(s => s.ReceivedGrade != null))
                        {
                            attempt.GradePercentage = (attempt.Solutions.Sum(s => s.ReceivedGrade) / attempt.Quiz.TotalPoints) * 100;
                        }
                    }

                    initializedJoinedAttempts.Add(attempt);
                }

                var quizzes = await _quizzesRepository.GetUserQuizzes();
                initializedCreatedQuizzes.Clear();
                foreach (var quiz in quizzes)
                {
                    initializedCreatedQuizzes.Add(quiz);
                }

                UpdateFilteredQuizzes();
                UpdateHasRunningAttempts();
            }, "Loading dashboard...");
        }

        public DashboardVM(IAttemptsRepository attemptsRepository, IQuizzesRepository quizzesRepository, INavigationService navigationService, IUIService uiService)
        {
            _attemptsRepository = attemptsRepository;
            _quizzesRepository = quizzesRepository;
            _navigationService = navigationService;
            _uiService = uiService;

            // Initialize collection properties to point to our backing fields
            JoinedAttempts = initializedJoinedAttempts;
            CreatedQuizzes = initializedCreatedQuizzes;

            // Set up subscriptions BEFORE initializing to catch any events
            _attemptsRepository.SubscribeCreate<ExamineeAttempt>(a =>
            {
                _uiService.BeginInvokeOnMainThread(() =>
                {
                    if (JoinedAttempts.FirstOrDefault(it => it.Id == a.Id) == null)
                    {
                        JoinedAttempts.Add(a);
                    }
                    // CollectionChanged event will trigger UpdateHasRunningAttempts
                });
            });
            _attemptsRepository.SubscribeUpdate<ExamineeAttempt>(a =>
            {
                _uiService.BeginInvokeOnMainThread(() =>
                {
                    var element = JoinedAttempts.FirstOrDefault(at => at.Id == a.Id);
                    if (element != null)
                    {
                        var idx = JoinedAttempts.IndexOf(element);
                        JoinedAttempts.Remove(element);
                        // Ensure GradePercentage is populated when available
                        if (a.GradePercentage == null && a.Solutions != null && a.Quiz != null && a.Quiz.TotalPoints > 0)
                        {
                            if (a.Solutions.All(s => s.ReceivedGrade != null))
                            {
                                a.GradePercentage = (a.Solutions.Sum(s => s.ReceivedGrade) / a.Quiz.TotalPoints) * 100;
                            }
                        }

                        JoinedAttempts.Insert(idx, a);
                    }
                    // CollectionChanged event will trigger UpdateHasRunningAttempts
                });
            });

            _quizzesRepository.SubscribeCreate<ExaminerQuiz>(q =>
            {
                _uiService.BeginInvokeOnMainThread(() =>
                {
                    if (CreatedQuizzes.FirstOrDefault(qu => qu.Id == q.Id) == null)
                        CreatedQuizzes.Add(q);
                    UpdateFilteredQuizzes();
                });
            });
            _quizzesRepository.SubscribeUpdate<ExaminerQuiz>(q =>
            {
                _uiService.BeginInvokeOnMainThread(() =>
                {
                    var element = CreatedQuizzes.FirstOrDefault(qu => qu.Id == q.Id);
                    if (element != null)
                    {
                        var idx = CreatedQuizzes.IndexOf(element);
                        CreatedQuizzes.Remove(element);
                        CreatedQuizzes.Insert(idx, q);
                    }
                    UpdateFilteredQuizzes();
                });
            });
            _quizzesRepository.SubscribeDetele<ExaminerQuiz>(q =>
            {
                _uiService.BeginInvokeOnMainThread(() =>
                {
                    // Remove from CreatedQuizzes
                    var element = CreatedQuizzes.FirstOrDefault(qu => qu.Id == q);
                    if (element != null)
                    {
                        CreatedQuizzes.Remove(element);
                    }

                    // Also remove any joined attempts that belong to the deleted quiz
                    var relatedAttempts = JoinedAttempts.Where(at => at.QuizId == q).ToList();
                    foreach (var ra in relatedAttempts)
                    {
                        JoinedAttempts.Remove(ra);
                    }

                    // Update filtered collections
                    UpdateFilteredQuizzes();
                });
            });
        }
    }
}
