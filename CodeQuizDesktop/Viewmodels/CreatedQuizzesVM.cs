using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui.Core.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class CreatedQuizzesVM : BaseViewModel
    {
        private readonly IQuizzesRepository _quizzesRepository;
        private readonly INavigationService _navigationService;

        private ObservableCollection<ExaminerQuiz> allExaminerQuizzes = [];

        public ObservableCollection<ExaminerQuiz> AllExaminerQuizzes
        {
            get { return allExaminerQuizzes; }
            set
            {
                allExaminerQuizzes = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreateQuizCommand { get => new Command(async () => await OnCreateQuizPage()); }
        public ICommand EditQuizCommand { get => new Command<ExaminerQuiz>(async (q) => await OnEditQuiz(q)); }
        public ICommand DeleteQuizCommand { get => new Command<ExaminerQuiz>(async (q) => await OnDeleteQuizAsync(q)); }
        public ICommand ViewQuizCommand { get => new Command<ExaminerQuiz>(async (q) => await OnViewQuiz(q)); }

        public async Task OnCreateQuizPage()
        {
            await _navigationService.GoToAsync(nameof(CreateQuiz));
        }

        public async Task OnEditQuiz(ExaminerQuiz examinerQuiz)
        {
            await _navigationService.GoToAsync(nameof(CreateQuiz), new Dictionary<string, object>
            {
                { "quizModel", examinerQuiz.QuizToModel },
                { "id", examinerQuiz.Id }
            });
        }

        public async Task OnDeleteQuizAsync(ExaminerQuiz examinerQuiz)
        {
            await ExecuteAsync(async () =>
            {
                await _quizzesRepository.DeleteQuiz(examinerQuiz.Id);
            }, "Deleting quiz...");
        }

        public async Task OnViewQuiz(ExaminerQuiz examinerQuiz)
        {
            await _navigationService.GoToAsync(nameof(ExaminerViewQuiz), new Dictionary<string, object> { { "quiz", examinerQuiz } });
        }

        public async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                var response = await _quizzesRepository.GetUserQuizzes();
                AllExaminerQuizzes = response.ToObservableCollection();
            }, "Loading quizzes...");
        }

        public CreatedQuizzesVM(IQuizzesRepository quizzesRepository, INavigationService navigationService)
        {
            _quizzesRepository = quizzesRepository;
            _navigationService = navigationService;

            _quizzesRepository.SubscribeCreate<ExaminerQuiz>(q =>
            {
                if (AllExaminerQuizzes.FirstOrDefault(qu => qu.Id == q.Id) == null)
                    AllExaminerQuizzes.Add(q);
            });
            _quizzesRepository.SubscribeUpdate<ExaminerQuiz>(q =>
            {
                var element = AllExaminerQuizzes.First(qu => qu.Id == q.Id);
                var idx = AllExaminerQuizzes.IndexOf(element);
                AllExaminerQuizzes.Remove(element);
                AllExaminerQuizzes.Insert(idx, q);
            });
            _quizzesRepository.SubscribeDetele<ExaminerQuiz>(q =>
            {
                var element = AllExaminerQuizzes.First(qu => qu.Id == q);
                var idx = AllExaminerQuizzes.IndexOf(element);
                AllExaminerQuizzes.Remove(element);
            });
        }
    }
}
