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
    public class JoinedQuizzesVM : BaseViewModel
    {
        private IAttemptsRepository _attemptsRepository;

        private ObservableCollection<ExamineeAttempt> allExamineeAttempts;

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
        public ICommand JoinQuizCommand { get => new Command(JoinQuiz); }

        private async void OpenJoinQuizPage()
        {
            await Shell.Current.GoToAsync("///JoinQuizPage");
        }

        private async void JoinQuiz()
        {
            if (QuizCode == "")
                return;

            var beginAttemptResponse = new BeginAttemptRequest() { QuizCode = this.QuizCode };
            var response = await _attemptsRepository.BeginAttempt(beginAttemptResponse);
            await Shell.Current.GoToAsync($"///JoinQuizPage", new Dictionary<string, object>{{"attempt", response! }});

        }

        private async void Intialize()
        {
            var response = await _attemptsRepository.GetUserAttempts();
            AllExamineeAttempts = response.ToObservableCollection();
            System.Diagnostics.Debug.WriteLine(response.Count);

        }

        public JoinedQuizzesVM(IAttemptsRepository attemptsRepository)
        {
            _attemptsRepository = attemptsRepository;
            Intialize();
        }

    }
}
