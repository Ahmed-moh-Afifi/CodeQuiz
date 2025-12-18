using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class CreatedQuizzesVM : BaseViewModel
    {
        private IQuizzesRepository _quizzesRepository;

        private ObservableCollection<ExaminerQuiz> allExaminerQuizzes;

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
        public ICommand DeleteQuizCommand { get => new Command<ExaminerQuiz>(OnDeleteQuiz); }
        public ICommand ViewQuizCommand { get => new Command<ExaminerQuiz>(OnViewQuiz); }

        private async void OnCreateQuizPage()
        {
            await Shell.Current.GoToAsync("///CreateQuizPage");
        }

        private async void OnEditQuiz(ExaminerQuiz examinerQuiz)
        {
            await Shell.Current.GoToAsync(nameof(EditQuiz), new Dictionary<string, object>
            {
                { "quiz", examinerQuiz }
            });
        }

        private async void OnDeleteQuiz(ExaminerQuiz examinerQuiz)
        {
            await _quizzesRepository.DeleteQuiz(examinerQuiz.Id);
        }
        private async void OnViewQuiz(ExaminerQuiz examinerQuiz)
        {
            await Shell.Current.GoToAsync($"///ExaminerViewQuizPage", new Dictionary<string, object> { { "quiz", examinerQuiz } });
        }
        
        private async void Intialize()
        {
            var response = await _quizzesRepository.GetUserQuizzes();
            AllExaminerQuizzes = response.ToObservableCollection();

        }
        public CreatedQuizzesVM(IQuizzesRepository quizzesRepository)
        {
            _quizzesRepository = quizzesRepository;
            Intialize();
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
