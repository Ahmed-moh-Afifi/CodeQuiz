using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui.Core.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class CreatedQuizzesVM : BaseViewModel
    {
        private readonly IQuizzesRepository _quizzesRepository;

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

        public ICommand CreateQuizCommand { get => new Command(OnCreateQuizPage); }
        public ICommand EditQuizCommand { get => new Command<ExaminerQuiz>(OnEditQuiz); }
        public ICommand DeleteQuizCommand { get => new Command<ExaminerQuiz>(async (q) => await OnDeleteQuizAsync(q)); }
        public ICommand ViewQuizCommand { get => new Command<ExaminerQuiz>(OnViewQuiz); }

        private async void OnCreateQuizPage()
        {
            await Shell.Current.GoToAsync(nameof(CreateQuiz));
        }

        private async void OnEditQuiz(ExaminerQuiz examinerQuiz)
        {
            await Shell.Current.GoToAsync(nameof(CreateQuiz), new Dictionary<string, object>
            {
                { "quizModel", examinerQuiz.QuizToModel },
                { "id", examinerQuiz.Id }
            });
        }

        private async Task OnDeleteQuizAsync(ExaminerQuiz examinerQuiz)
        {
            await ExecuteAsync(async () =>
            {
                await _quizzesRepository.DeleteQuiz(examinerQuiz.Id);
            }, "Deleting quiz...");
        }

        private async void OnViewQuiz(ExaminerQuiz examinerQuiz)
        {
            await Shell.Current.GoToAsync(nameof(ExaminerViewQuiz), new Dictionary<string, object> { { "quiz", examinerQuiz } });
        }

        private async void InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                var response = await _quizzesRepository.GetUserQuizzes();
                AllExaminerQuizzes = response.ToObservableCollection();
            }, "Loading quizzes...");
        }

        public CreatedQuizzesVM(IQuizzesRepository quizzesRepository)
        {
            _quizzesRepository = quizzesRepository;
            InitializeAsync();
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
