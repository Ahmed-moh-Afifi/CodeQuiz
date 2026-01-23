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
        private readonly IUIService _uiService;
        private ExamineeAttempt? attempt;
        public ExamineeAttempt? Attempt
        {
            get => attempt;
            set
            {
                attempt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(QuizProgress));
            }
        }

        private TimeSpan remainingTime;
        public TimeSpan RemainingTime
        {
            get { return remainingTime; }
            set
            {
                remainingTime = new TimeSpan(value.Hours, value.Minutes, value.Seconds);
                if (value.TotalMilliseconds == 0)
                {
                    // Time's up - trigger the async submission sequence
                    _ = HandleTimeExpiredAsync();
                }
                OnPropertyChanged();
            }
        }

        private bool submitting = false;
        public bool Submitting
        {
            get { return submitting; }
            set
            {
                submitting = value;
                if (value)
                {
                    System.Diagnostics.Debug.WriteLine("Submitting Set to TRUE");
                }
                OnPropertyChanged();
            }
        }

        private bool isSavingManually = false;
        /// <summary>
        /// Indicates if a manual save operation is in progress (triggered by navigation)
        /// </summary>
        public bool IsSavingManually
        {
            get { return isSavingManually; }
            set
            {
                isSavingManually = value;
                OnPropertyChanged();
            }
        }

        private bool isAutoSaving = false;
        /// <summary>
        /// Indicates if an auto-save operation is in progress
        /// </summary>
        public bool IsAutoSaving
        {
            get { return isAutoSaving; }
            set
            {
                isAutoSaving = value;
                OnPropertyChanged();
            }
        }

        private DateTime? lastSavedTime;
        /// <summary>
        /// The last time the solution was saved
        /// </summary>
        public DateTime? LastSavedTime
        {
            get { return lastSavedTime; }
            set
            {
                lastSavedTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastSavedText));
            }
        }

        /// <summary>
        /// Formatted text for the last saved time
        /// </summary>
        public string LastSavedText
        {
            get
            {
                if (LastSavedTime == null)
                    return "Not saved yet";

                var timeSince = DateTime.Now - LastSavedTime.Value;
                if (timeSince.TotalSeconds < 5)
                    return "Just now";
                if (timeSince.TotalSeconds < 60)
                    return $"{(int)timeSince.TotalSeconds}s ago";
                if (timeSince.TotalMinutes < 60)
                    return $"{(int)timeSince.TotalMinutes}m ago";
                return LastSavedTime.Value.ToString("HH:mm");
            }
        }

        /// <summary>
        /// Progress through the quiz (0.0 to 1.0) based on current question
        /// </summary>
        public double QuizProgress
        {
            get
            {
                if (Attempt?.Quiz?.Questions == null || Attempt.Quiz.Questions.Count == 0 || SelectedQuestion == null)
                    return 0;
                return (double)SelectedQuestion.Order / Attempt.Quiz.Questions.Count;
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
                OnPropertyChanged(nameof(QuizProgress));
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

        /// <summary>
        /// Flag to prevent multiple simultaneous auto-save operations
        /// </summary>
        private bool _isAutoSaving = false;

        /// <summary>
        /// Flag to prevent multiple simultaneous submission operations
        /// </summary>
        private bool _isSubmitting = false;

        public EditorType EditorTypeValue
        {
            get => editorTypeValue;
            set
            {
                editorTypeValue = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand ReturnCommand { get => new Command(async () => await ReturnToPreviousPageAsync()); }
        public ICommand SubmitQuizCommand { get => new Command(async () => await SubmitQuizAsync()); }
        public ICommand NextQuestionCommand { get => new Command(async () => await NextQuestionAsync()); }
        public ICommand PreviousQuestionCommand { get => new Command(async () => await PreviousQuestionAsync()); }
        public ICommand SpecificQuestionCommand { get => new Command<Question>(async (q) => await SpecificQuestionAsync(q)); }
        public ICommand RunCommand { get => new Command(async () => await RunAsync()); }

        // Remaining Time Timer
        IDispatcherTimer? dispatcherTimer;
        private EditorType editorTypeValue;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("attempt") && query["attempt"] is ExamineeAttempt receivedAttempt)
            {
                Attempt = receivedAttempt;
                //_attemptsRepository.SubscribeUpdate(CheckAndUpdate);
                SelectedQuestion = Attempt.Quiz.Questions.Find(q => q.Order == 1);
                CalculateRemainingTime();
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
                        Submitting = false;
                        _isSubmitting = false;
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

            // MaxEndTime is in UTC (from backend), compare with current UTC time
            var tmpRemainingTime = Attempt!.MaxEndTime.Subtract(DateTime.UtcNow);
            RemainingTime = tmpRemainingTime.TotalSeconds > 0 ? tmpRemainingTime : TimeSpan.Zero;
        }

        public async Task ReturnToPreviousPageAsync()
        {
            await _navigationService.GoToAsync("///MainPage");
        }

        /// <summary>
        /// Saves the current solution asynchronously and waits for the operation to complete.
        /// Returns true if save was successful, false otherwise.
        /// </summary>
        private async Task<bool> SaveSolutionAsync()
        {
            try
            {
                Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.Code = CodeInEditor;
                await _attemptsRepository.UpdateSolution(Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!);
                System.Diagnostics.Debug.WriteLine("Solution saved successfully");
                LastSavedTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save solution: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Handles the debounced auto-save event from the CodeEditor.
        /// Saves the current solution when the user stops typing for 2 seconds.
        /// </summary>
        public async void OnAutoSaveTriggered(object? sender, EventArgs e)
        {
            // Prevent multiple simultaneous saves
            if (_isAutoSaving || Submitting || IsSavingManually || _isSubmitting)
                return;

            try
            {
                _isAutoSaving = true;
                IsAutoSaving = true;
                System.Diagnostics.Debug.WriteLine("Auto-save triggered (user stopped typing)");
                await SaveSolutionAsync();
            }
            finally
            {
                _isAutoSaving = false;
                IsAutoSaving = false;
            }
        }

        /// <summary>
        /// Handles the time expiration sequence using "Frontend-First" submission strategy.
        /// When the timer reaches zero:
        /// 1. Immediately lock the UI
        /// 2. Save any pending changes
        /// 3. Call SubmitAttempt directly (frontend is the primary submission actor)
        /// </summary>
        private async Task HandleTimeExpiredAsync()
        {
            // Prevent duplicate submission
            if (_isSubmitting)
            {
                System.Diagnostics.Debug.WriteLine("HandleTimeExpiredAsync: Already submitting, skipping");
                return;
            }

            _isSubmitting = true;

            // 1. Immediately lock the UI
            Submitting = true;

            // Stop the timer to prevent further ticks
            dispatcherTimer?.Stop();

            try
            {
                // 2. Save any pending changes (best effort - don't fail if this fails)
                System.Diagnostics.Debug.WriteLine("Time expired - saving final solution before submit");
                await SaveSolutionAsync();
            }
            catch (Exception ex)
            {
                // Log but don't prevent submission - data loss is acceptable if save fails at deadline
                System.Diagnostics.Debug.WriteLine($"Failed to save final solution: {ex.Message}");
            }

            try
            {
                // 3. Frontend-First: Submit the attempt directly from the client
                System.Diagnostics.Debug.WriteLine("Submitting attempt from frontend (Frontend-First strategy)");
                await _attemptsRepository.SubmitAttempt(Attempt!.Id);

                Submitting = false;
                await ReturnToPreviousPageAsync();
            }
            catch (Exception ex)
            {
                // If submission fails, the backend's AttemptTimerService will eventually auto-submit
                System.Diagnostics.Debug.WriteLine($"Frontend submission failed, backend will handle: {ex.Message}");
                Submitting = false;
                await ReturnToPreviousPageAsync();
            }
            finally
            {
                _isSubmitting = false;
            }
        }

        private async Task SubmitQuizAsync()
        {
            // Prevent duplicate submission
            if (_isSubmitting || Submitting)
            {
                System.Diagnostics.Debug.WriteLine("SubmitQuizAsync: Already submitting, skipping");
                return;
            }

            // Show confirmation dialog before submitting
            var confirmed = await _uiService.ShowConfirmationAsync(
                "Submit Quiz",
                "Are you sure you want to submit your quiz? You won't be able to make any more changes after submission.",
                "Submit",
                "Cancel");

            if (!confirmed)
                return;

            // Set flags BEFORE showing loading indicator to prevent race conditions
            _isSubmitting = true;
            //Submitting = true;

            // Stop the timer to prevent time-based submission during manual submission
            dispatcherTimer?.Stop();

            try
            {
                // Show loading indicator manually instead of using ExecuteAsync to avoid double loading
                if (UIService != null)
                    await UIService.ShowLoadingAsync("Submitting quiz...");

                // Await save to ensure changes are persisted before submit
                await SaveSolutionAsync();
                await _attemptsRepository.SubmitAttempt(Attempt!.Id);

                if (UIService != null)
                    await UIService.HideLoadingAsync();

                await ReturnToPreviousPageAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SubmitQuizAsync failed: {ex.Message}");
                if (UIService != null)
                    await UIService.HideLoadingAsync();

                await _uiService.ShowErrorAsync($"Failed to submit quiz: {ex.Message}");
            }
            finally
            {
                _isSubmitting = false;
                Submitting = false;
            }
        }

        private async Task NextQuestionAsync()
        {
            if (IsSavingManually || Submitting || _isSubmitting)
                return;

            try
            {
                IsSavingManually = true;
                await SaveSolutionAsync();

                if (SelectedQuestion!.Order + 1 <= Attempt!.Quiz.Questions.Count)
                {
                    SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == SelectedQuestion!.Order + 1);
                }
            }
            finally
            {
                IsSavingManually = false;
            }
        }

        private async Task PreviousQuestionAsync()
        {
            if (IsSavingManually || Submitting || _isSubmitting)
                return;

            try
            {
                IsSavingManually = true;
                await SaveSolutionAsync();

                if (SelectedQuestion!.Order - 1 > 0)
                {
                    SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == SelectedQuestion!.Order - 1);
                }
            }
            finally
            {
                IsSavingManually = false;
            }
        }

        private async Task SpecificQuestionAsync(Question question)
        {
            if (IsSavingManually || Submitting || _isSubmitting)
                return;

            try
            {
                IsSavingManually = true;
                await SaveSolutionAsync();
                SelectedQuestion = question;
            }
            finally
            {
                IsSavingManually = false;
            }
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
                    Input = (this.Input).Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries).ToList()
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

        public JoinQuizVM(IAttemptsRepository attemptsRepository, IExecutionRepository executionRepository, INavigationService navigationService, IUIService uiService)
        {
            _attemptsRepository = attemptsRepository;
            _executionRepository = executionRepository;
            _navigationService = navigationService;
            _uiService = uiService;
            _attemptsRepository.SubscribeUpdate<ExamineeAttempt>(a => Attempt = a);
        }
    }
}