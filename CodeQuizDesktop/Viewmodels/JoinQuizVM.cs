using CodeQuizDesktop.Controls;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class JoinQuizVM : BaseViewModel, IQueryAttributable
    {
        private readonly INavigationService _navigationService;
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

        private TimeSpan remainingTime;
        public TimeSpan RemainingTime
        {
            get { return remainingTime; }
            set
            {
                remainingTime = new TimeSpan(value.Hours, value.Minutes, value.Seconds);
                if (value.Hours == 0 && value.Minutes == 0 && value.Seconds == 0)
                {
                    WaitingForAutoSubmission = true;
                    SaveSolution();
                }
                OnPropertyChanged();
            }
        }

        private bool waitingForAutoSubmission = false;
        public bool WaitingForAutoSubmission
        {
            get { return waitingForAutoSubmission; }
            set
            {
                waitingForAutoSubmission = value;
                if (value)
                {
                    System.Diagnostics.Debug.WriteLine("WaitingForAutoSubmission Set to TRUE");
                }
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
                if (selectedQuestion!.QuestionConfiguration.AllowIntellisense && selectedQuestion.QuestionConfiguration.AllowSignatureHelp)
                {
                    EditorTypeValue = EditorType.AllHelpers;
                }
                else if (!(selectedQuestion.QuestionConfiguration.AllowIntellisense) && selectedQuestion.QuestionConfiguration.AllowSignatureHelp)
                {
                    EditorTypeValue = EditorType.SignatureOnly;
                }
                else if (selectedQuestion.QuestionConfiguration.AllowIntellisense && !(selectedQuestion.QuestionConfiguration.AllowSignatureHelp))
                {
                    EditorTypeValue = EditorType.IntellisenseOnly;
                }
                else
                {
                    EditorTypeValue = EditorType.NoHelpers;
                }

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

        private bool isRunningCode;
        public bool IsRunningCode
        {
            get { return isRunningCode; }
            set
            {
                isRunningCode = value;
                OnPropertyChanged();
            }
        }

        public EditorType EditorTypeValue { get; set; }

        // Commands
        public ICommand ReturnCommand { get => new Command(async () => await ReturnToPreviousPageAsync()); }
        public ICommand SubmitQuizCommand { get => new Command(async () => await SubmitQuizAsync()); }
        public ICommand NextQuestionCommand { get => new Command(NextQuestion); }
        public ICommand PreviousQuestionCommand { get => new Command(PreviousQuestion); }
        public ICommand SpecificQuestionCommand { get => new Command<Question>(SpecificQuestion); }
        public ICommand RunCommand { get => new Command(async () => await RunAsync()); }

        // Remaining Time Timer
        IDispatcherTimer? dispatcherTimer;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("attempt") && query["attempt"] is ExamineeAttempt receivedAttempt)
            {
                Attempt = receivedAttempt;
                _attemptsRepository.SubscribeUpdate(CheckAndUpdate);
                CalculateRemainingTime();
                SelectedQuestion = Attempt.Quiz.Questions.Find(q => q.Order == 1);
            }
        }

        public void CheckAndUpdate(ExamineeAttempt attempt)
        {
            if (attempt.Id == Attempt!.Id)
            {
                Attempt = attempt;
                if (Attempt.EndTime != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        dispatcherTimer?.Stop();
                        dispatcherTimer = null;
                        WaitingForAutoSubmission = false;
                        await ReturnToPreviousPageAsync();
                    });
                }
            }
        }

        public void CalculateRemainingTime()
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = Application.Current!.Dispatcher.CreateTimer();
                dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
                dispatcherTimer.Tick += (s, e) => CalculateRemainingTime();
                dispatcherTimer.Start();
            }

            var tmpRemainingTime = Attempt!.MaxEndTime.Subtract(DateTime.Now);
            RemainingTime = tmpRemainingTime.TotalSeconds > 0 ? tmpRemainingTime : TimeSpan.Zero;
        }

        public async Task ReturnToPreviousPageAsync()
        {
            await _navigationService.GoToAsync("///MainPage");
        }

        private void SaveSolution()
        {
            Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.Code = CodeInEditor;
            _attemptsRepository.UpdateSolution(Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!);
        }

        private async Task SubmitQuizAsync()
        {
            await ExecuteAsync(async () =>
            {
                SaveSolution();
                var response = await _attemptsRepository.SubmitAttempt(Attempt!.Id);
            }, "Submitting quiz...");
            await ReturnToPreviousPageAsync();
        }

        private void NextQuestion()
        {
            SaveSolution();
            if (SelectedQuestion!.Order + 1 <= Attempt!.Quiz.Questions.Count)
            {
                SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == SelectedQuestion!.Order + 1);
            }
        }

        private void PreviousQuestion()
        {
            SaveSolution();
            if (SelectedQuestion!.Order - 1 > 0)
            {
                SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == SelectedQuestion!.Order - 1);
            }
        }

        private void SpecificQuestion(Question question)
        {
            SaveSolution();
            SelectedQuestion = question;
        }

        private async Task RunAsync()
        {
            if (IsRunningCode)
                return;

            try
            {
                IsRunningCode = true;
                Output = "Running...";

                var runCodeRequest = new RunCodeRequest()
                {
                    Language = SelectedQuestion!.QuestionConfiguration.Language,
                    ContainOutput = SelectedQuestion!.QuestionConfiguration.ShowOutput,
                    ContainError = SelectedQuestion!.QuestionConfiguration.ShowError,
                    Code = CodeInEditor,
                    Input = (this.Input).Split(['\n', ' ', '\r']).ToList()
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
            finally
            {
                IsRunningCode = false;
            }
        }

        private readonly IAttemptsRepository _attemptsRepository;
        private readonly IExecutionRepository _executionRepository;

        public JoinQuizVM(IAttemptsRepository attemptsRepository, IExecutionRepository executionRepository, INavigationService navigationService)
        {
            _attemptsRepository = attemptsRepository;
            _executionRepository = executionRepository;
            _navigationService = navigationService;
            _attemptsRepository.SubscribeUpdate(a => Attempt = a);
        }
    }
}