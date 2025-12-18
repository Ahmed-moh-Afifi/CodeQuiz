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
    public class ExaminerViewQuizVM : BaseViewModel, IQueryAttributable
    {
        private IQuizzesRepository _quizzesRepository;

        private ExaminerQuiz quiz;

        public ExaminerQuiz Quiz
        {
            get { return quiz; }
            set
            {
                quiz = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ExaminerAttempt> attempts;

        public ObservableCollection<ExaminerAttempt> Attempts
        {
            get { return attempts; }
            set
            {
                attempts = value;
                OnPropertyChanged();
            }
        }

        public ICommand ReturnCommand { get => new Command(ReturnToPreviousPage); }

        private async void ReturnToPreviousPage()
        {
            await Shell.Current.GoToAsync("..");
        }

        public ICommand GoToGradeAttemptPageCommand { get => new Command<ExaminerAttempt>(OnGoToGradeAttemptPage); }
        private async void OnGoToGradeAttemptPage(ExaminerAttempt examinerAttempt)
        {
            await Shell.Current.GoToAsync(nameof(GradeAttempt), new Dictionary<string, object> 
            { 
                { "attempt", examinerAttempt! },
                { "quiz", Quiz!  }
            });

        }
        

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("quiz") && query["quiz"] is ExaminerQuiz receivedQuiz)
            {
                Quiz = receivedQuiz;
                var response = await _quizzesRepository.GetQuizAttempts(Quiz.Id);
                Attempts = response.ToObservableCollection();
            }
        }

        public ExaminerViewQuizVM(IQuizzesRepository quizzesRepository)
        {
            _quizzesRepository = quizzesRepository;
            //subscribe update

        }
    }
}
