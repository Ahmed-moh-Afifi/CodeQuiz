using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
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
        public ICommand CreateQuizCommand { get => new Command(OpenCreateQuizPage); }
        //public ICommand EditQuizCommand { get => new Command<ExaminerQuiz>(OnEditQuiz); }
        public ICommand DeleteQuizCommand { get => new Command<ExaminerQuiz>(OnDeleteQuiz); }

        private async void OpenCreateQuizPage()
        {
            await Shell.Current.GoToAsync("///CreateQuizPage");
        }

        public ICommand ViewQuizCommand { get => new Command(OpenExaminerViewQuizPage); }

        private async void OpenExaminerViewQuizPage()
        {
            await Shell.Current.GoToAsync("///ExaminerViewQuizPage");
        }
        //private async void OnEditQuiz(ExaminerQuiz examinerQuiz)
        //{
        //    await _quizzesRepository.UpdateQuiz(examinerQuiz);
        //}

        private async void OnDeleteQuiz(ExaminerQuiz examinerQuiz)
        {
            await _quizzesRepository.DeleteQuiz(examinerQuiz.Id);
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
