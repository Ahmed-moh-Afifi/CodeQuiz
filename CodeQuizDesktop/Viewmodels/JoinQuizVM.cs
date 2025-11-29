using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class JoinQuizVM : BaseViewModel, IQueryAttributable
    {
        private ExamineeAttempt? attempt;
        public ExamineeAttempt? Attempt 
        {
            get => attempt;
            set
            { 
                attempt = value;
                OnPropertyChanged();
            }
        }

        private Question? selectedQuestion;

        public Question? SelectedQuestion
        {
            get
            {
                return selectedQuestion;
            }
            set
            {
                selectedQuestion = value;
                HasTestCases = SelectedQuestion!.TestCases.Count != 0;
                CodeInEditor = Attempt!.Solutions[value!.Order - 1].Code;

                OnPropertyChanged();
            }
        }

        private bool hasTestCases;

        public bool HasTestCases
        {
            get { return hasTestCases; }
            set 
            { 
                hasTestCases = value;
                OnPropertyChanged();
            }
        }

        private string codeInEditor = "";
        public string CodeInEditor
        {
            get => codeInEditor;
            set
            {
                codeInEditor = value;
                OnPropertyChanged();
            }
        }

        private string input = "";

        public string Input
        {
            get { return input; }
            set
            {
                input = value;
                OnPropertyChanged();
            }
        }

        private string? output;

        public string? Output
        {
            get { return output; }
            set
            {
                output = value;
                OnPropertyChanged();
            }
        }

        public ICommand ReturnCommand { get => new Command(ReturnToPreviousPage); }

        public ICommand SubmitQuizCommand { get => new Command(SubmitQuiz); }

        public ICommand NextQuestionCommand { get => new Command(NextQuestion); }
        public ICommand PreviousQuestionCommand { get => new Command(PreviousQuestion); }
        public ICommand RunCommand { get => new Command(Run); }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("attempt") && query["attempt"] is ExamineeAttempt receivedAttempt)
            {
                Attempt = receivedAttempt;
                SelectedQuestion = Attempt.Quiz.Questions.Find(q => q.Order == 1);
            }
        }

        private async void ReturnToPreviousPage()
        {
            await Shell.Current.GoToAsync("///MainPage");

        }

        private async void SubmitQuiz()
        {

            var response = await _attemptsRepository.SubmitAttempt(Attempt!.Id);
            await Shell.Current.GoToAsync("///MainPage");
        }

        private void NextQuestion()
        {
            if (SelectedQuestion!.Order + 1 <= Attempt!.Quiz.Questions.Count)
            {
                SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == SelectedQuestion!.Order + 1);
            }
        }

        private void PreviousQuestion()
        {
            if (SelectedQuestion!.Order - 1 > 0)
            {
                SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == SelectedQuestion!.Order - 1);
            }
        }

        private void OpenSpecificQuestion()
        {
            
        }

        private async void Run()
        {
            var runCodeRequest = new RunCodeRequest()
            {
                Language = SelectedQuestion!.QuestionConfiguration.Language,
                ContainOutput = SelectedQuestion!.QuestionConfiguration.ShowOutput,
                ContainError = SelectedQuestion!.QuestionConfiguration.ShowError,
                
                Code = CodeInEditor,
                
                Input = (this.Input).Split('\n').ToList()

            };
            
            var response = await _executionRepository.RunCode(runCodeRequest);
            if (SelectedQuestion.QuestionConfiguration.AllowExecution)
            {
                if (response.Success)
                {
                    if (SelectedQuestion.QuestionConfiguration.ShowOutput)
                    {
                        Output = response.Output!;
                    }
                    else
                    {
                        Output = "Code executed successfully";
                    }
                }
                else
                {
                    if (SelectedQuestion.QuestionConfiguration.ShowError)
                    {
                        Output = response.Error!;
                    }
                    else
                    {
                        Output = "Code execution failed";
                    }
                }
            }

        }

        private IAttemptsRepository _attemptsRepository;
        private IExecutionRepository _executionRepository;

        public JoinQuizVM(IAttemptsRepository attemptsRepository, IExecutionRepository executionRepository)
        {
            _attemptsRepository = attemptsRepository;
            _executionRepository = executionRepository;
            _attemptsRepository.SubscribeUpdate(a => Attempt = a);
        }
    }
}