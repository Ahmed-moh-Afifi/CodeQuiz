using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Graphics;
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

        public ICommand ContinueAttemptCommand { get => new Command<ExamineeAttempt>(OnContinueAttempt); }


        private async void OnContinueAttempt(ExamineeAttempt examineeAttempt)
        {
            System.Diagnostics.Debug.WriteLine($"Clicked: {examineeAttempt.Quiz.Code}");
            var beginAttemptResponse = new BeginAttemptRequest() { QuizCode = examineeAttempt.Quiz.Code };
            var response = await _attemptsRepository.BeginAttempt(beginAttemptResponse);
            await Shell.Current.GoToAsync($"///JoinQuizPage", new Dictionary<string, object> { { "attempt", response! } });

        }

        private async void JoinQuiz()
        {
            if (QuizCode == "")
                return;

            var beginAttemptResponse = new BeginAttemptRequest() { QuizCode = this.QuizCode };
            var response = await _attemptsRepository.BeginAttempt(beginAttemptResponse);
            await Shell.Current.GoToAsync($"///JoinQuizPage", new Dictionary<string, object> { { "attempt", response! } });

        }

        private async void Intialize()
        {
            var response = await _attemptsRepository.GetUserAttempts();
            AllExamineeAttempts = response.ToObservableCollection();

        }

        public JoinedQuizzesVM(IAttemptsRepository attemptsRepository)
        {
            _attemptsRepository = attemptsRepository;
            Intialize();
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
