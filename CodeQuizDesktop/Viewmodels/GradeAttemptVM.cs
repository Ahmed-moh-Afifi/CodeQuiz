using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class GradeAttemptVM : BaseViewModel, IQueryAttributable
    {
        private ExaminerAttempt? attempt;
        public ExaminerAttempt? Attempt
        {
            get => attempt;
            set
            {
                attempt = value;
                OnPropertyChanged();
            }
        }

        private ExaminerQuiz? quiz;

        public ExaminerQuiz? Quiz
        {
            get { return quiz; }
            set
            {
                quiz = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Dictionary to track the original grades from the server for each solution.
        /// Key: SolutionId, Value: Original grade (null if not graded)
        /// </summary>
        private readonly Dictionary<int, float?> _originalGrades = new();

        /// <summary>
        /// Dictionary to track the current (possibly modified) grades for each solution.
        /// Key: SolutionId, Value: Current grade
        /// </summary>
        private readonly Dictionary<int, float?> _currentGrades = new();

        /// <summary>
        /// Dictionary to track feedback comments for each solution.
        /// Key: SolutionId, Value: Feedback text
        /// </summary>
        private readonly Dictionary<int, string?> _currentFeedback = new();

        /// <summary>
        /// Dictionary to track original feedback for comparison.
        /// Key: SolutionId, Value: Original feedback text
        /// </summary>
        private readonly Dictionary<int, string?> _originalFeedback = new();

        private Question? selectedQuestion;
        public Question? SelectedQuestion
        {
            get
            {
                return selectedQuestion;
            }
            set
            {
                // Save current question's grade and feedback to tracking dictionary before switching
                if (selectedQuestion != null && Attempt != null)
                {
                    var currentSol = Attempt.Solutions.Find(s => s.QuestionId == selectedQuestion.Id);
                    if (currentSol != null)
                    {
                        _currentGrades[currentSol.Id] = Grade;
                        _currentFeedback[currentSol.Id] = Feedback;
                    }
                }

                selectedQuestion = value;
                HasTestCases = SelectedQuestion!.TestCases.Count != 0;

                // Find solution by QuestionId, not by index (which may not match question order)
                var solution = Attempt!.Solutions.Find(s => s.QuestionId == value!.Id);
                CodeInEditor = solution?.Code ?? "";

                // Load grade from tracking dictionary if available, otherwise from solution
                if (solution != null && _currentGrades.TryGetValue(solution.Id, out var trackedGrade))
                {
                    Grade = trackedGrade;
                }
                else
                {
                    Grade = solution?.ReceivedGrade;
                }

                // Load feedback from tracking dictionary if available, otherwise from solution
                if (solution != null && _currentFeedback.TryGetValue(solution.Id, out var trackedFeedback))
                {
                    Feedback = trackedFeedback;
                }
                else
                {
                    Feedback = solution?.Feedback;
                }

                // Update AI assessment for the selected question
                UpdateAiAssessment();

                OnPropertyChanged();
            }
        }

        #region AI Assessment Properties

        private AiAssessment? currentAiAssessment;
        /// <summary>
        /// The AI assessment for the currently selected question's solution.
        /// </summary>
        public AiAssessment? CurrentAiAssessment
        {
            get => currentAiAssessment;
            set
            {
                currentAiAssessment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasAiAssessment));
                OnPropertyChanged(nameof(HasFlags));
                OnPropertyChanged(nameof(AiValidityText));
                OnPropertyChanged(nameof(AiConfidenceText));
                // Notify suggested grade properties
                OnPropertyChanged(nameof(SuggestedGrade));
                OnPropertyChanged(nameof(SuggestedGradeActual));
                OnPropertyChanged(nameof(HasSuggestedGrade));
                OnPropertyChanged(nameof(SuggestedGradeText));
            }
        }

        /// <summary>
        /// Whether the current solution has an AI assessment.
        /// </summary>
        public bool HasAiAssessment => CurrentAiAssessment != null;

        /// <summary>
        /// Whether the current AI assessment has any flags.
        /// </summary>
        public bool HasFlags => CurrentAiAssessment?.Flags?.Any() == true;

        /// <summary>
        /// User-friendly text for AI validity status.
        /// </summary>
        public string AiValidityText => CurrentAiAssessment?.IsValid == true ? "Valid" : "Suspicious";

        /// <summary>
        /// User-friendly text for AI confidence score.
        /// </summary>
        public string AiConfidenceText => CurrentAiAssessment != null
            ? $"{(CurrentAiAssessment.ConfidenceScore * 100):F0}% confidence"
            : "";

        /// <summary>
        /// The AI suggested grade for the current solution (0.0 - 1.0 scale).
        /// </summary>
        public float? SuggestedGrade => CurrentAiAssessment?.SuggestedGrade;

        /// <summary>
        /// The AI suggested grade as actual points (not percentage).
        /// </summary>
        public float? SuggestedGradeActual => SuggestedGrade.HasValue && SelectedQuestion != null
            ? SuggestedGrade.Value * SelectedQuestion.Points
            : null;

        /// <summary>
        /// Whether there is a suggested grade from the AI.
        /// </summary>
        public bool HasSuggestedGrade => SuggestedGrade.HasValue;

        /// <summary>
        /// User-friendly text for the AI suggested grade.
        /// </summary>
        public string SuggestedGradeText => SuggestedGrade.HasValue
            ? $"AI Suggested: {SuggestedGrade.Value:P0}"
            : "";

        private string? evaluationStatus;
        /// <summary>
        /// Current evaluation status message.
        /// </summary>
        public string? EvaluationStatus
        {
            get => evaluationStatus;
            set
            {
                evaluationStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEvaluating));
            }
        }

        /// <summary>
        /// Whether an evaluation is currently in progress.
        /// </summary>
        public bool IsEvaluating => !string.IsNullOrEmpty(EvaluationStatus) && !EvaluationStatus.Contains("complete", StringComparison.OrdinalIgnoreCase);

        private Solution? currentSolution;
        /// <summary>
        /// The current solution for the selected question.
        /// </summary>
        public Solution? CurrentSolution
        {
            get => currentSolution;
            set
            {
                currentSolution = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Updates the AI assessment when the selected question changes.
        /// </summary>
        private void UpdateAiAssessment()
        {
            if (SelectedQuestion == null || Attempt == null)
            {
                CurrentAiAssessment = null;
                CurrentSolution = null;
                return;
            }

            CurrentSolution = Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion.Id);
            CurrentAiAssessment = CurrentSolution?.AiAssessment; // This triggers OnPropertyChanged for suggested grade properties
        }

        #endregion

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

        private float? grade;

        public float? Grade
        {
            get { return grade; }
            set
            {
                if (grade != value)
                {
                    grade = value;
                    UpdateHasUnsavedChanges();
                    OnPropertyChanged();
                }
            }
        }

        private string? feedback;
        /// <summary>
        /// Feedback comment for the current solution.
        /// </summary>
        public string? Feedback
        {
            get { return feedback; }
            set
            {
                if (feedback != value)
                {
                    feedback = value;
                    UpdateHasUnsavedChanges();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Updates HasUnsavedChanges based on whether any solution has modified grades or feedback.
        /// </summary>
        private void UpdateHasUnsavedChanges()
        {
            if (Attempt == null)
            {
                HasUnsavedChanges = false;
                return;
            }

            // Check if current question has changes
            var currentSol = SelectedQuestion != null
                ? Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion.Id)
                : null;

            if (currentSol != null)
            {
                if (_originalGrades.TryGetValue(currentSol.Id, out var origGrade) && Grade != origGrade)
                {
                    HasUnsavedChanges = true;
                    return;
                }
                if (_originalFeedback.TryGetValue(currentSol.Id, out var origFeedback) && Feedback != origFeedback)
                {
                    HasUnsavedChanges = true;
                    return;
                }
            }

            // Check all tracked solutions
            foreach (var kvp in _currentGrades)
            {
                if (_originalGrades.TryGetValue(kvp.Key, out var origGrade) && kvp.Value != origGrade)
                {
                    HasUnsavedChanges = true;
                    return;
                }
            }

            foreach (var kvp in _currentFeedback)
            {
                if (_originalFeedback.TryGetValue(kvp.Key, out var origFeedback) && kvp.Value != origFeedback)
                {
                    HasUnsavedChanges = true;
                    return;
                }
            }

            HasUnsavedChanges = false;
        }

        private bool _hasUnsavedChanges;
        /// <summary>
        /// Tracks whether there are unsaved grade changes.
        /// </summary>
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                _hasUnsavedChanges = value;
                OnPropertyChanged();
            }
        }

        private Action<ExaminerAttempt>? _attemptUpdatedHandler;
        private Action<EvaluationStatusPayload>? _evaluationStatusHandler;

        //Commands
        public ICommand ReturnCommand { get => new Command(async () => await ReturnToPreviousPage()); }
        public ICommand NextQuestionCommand { get => new Command(NextQuestion); }
        public ICommand PreviousQuestionCommand { get => new Command(PreviousQuestion); }
        public ICommand SpecificQuestionCommand { get => new Command<Question>(SpecificQuestion); }
        public ICommand RunCommand { get => new Command(async () => await Run()); }
        public ICommand SaveAllGradesCommand { get => new Command(async () => await SaveAllGradesAndGoBack()); }
        public ICommand RerunAiAssessmentCommand { get => new Command(async () => await RerunAiAssessment(), () => CanRerunAiAssessment); }

        private bool _isRerunningAiAssessment;
        /// <summary>
        /// Whether an AI re-assessment is currently in progress.
        /// </summary>
        public bool IsRerunningAiAssessment
        {
            get => _isRerunningAiAssessment;
            set
            {
                _isRerunningAiAssessment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRerunAiAssessment));
            }
        }

        /// <summary>
        /// Whether the AI re-assessment command can be executed.
        /// </summary>
        public bool CanRerunAiAssessment => CurrentSolution != null && !IsRerunningAiAssessment;

        /// <summary>
        /// Re-runs AI assessment for the current solution.
        /// </summary>
        private async Task RerunAiAssessment()
        {
            if (CurrentSolution == null) return;

            try
            {
                IsRerunningAiAssessment = true;
                EvaluationStatus = "Queuing AI reassessment...";

                await _attemptsRepository.RerunAiAssessment(CurrentSolution.Id);

                EvaluationStatus = "AI reassessment in progress...";
                System.Diagnostics.Debug.WriteLine($"AI reassessment queued for solution {CurrentSolution.Id}");
            }
            catch (Exception ex)
            {
                EvaluationStatus = $"AI reassessment failed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Failed to queue AI reassessment: {ex.Message}");

                // Clear status after delay on error
                await Task.Delay(3000);
                if (EvaluationStatus?.Contains("failed") == true)
                    EvaluationStatus = null;
            }
            finally
            {
                IsRerunningAiAssessment = false;
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("attempt") && query["attempt"] is ExaminerAttempt receivedAttempt)
            {
                Attempt = receivedAttempt;

                // Initialize original grades and feedback tracking
                InitializeOriginalValues();

                SubscribeToRealTimeUpdates();
            }
            if (query.ContainsKey("quiz") && query["quiz"] is ExaminerQuiz receivedQuiz)
            {
                Quiz = receivedQuiz;
                SelectedQuestion = Quiz!.Questions.Find(q => q.Order == 1);
                // Grade is now set inside SelectedQuestion setter
                System.Diagnostics.Debug.WriteLine($"Quiz ID: {receivedQuiz.Id}");
            }
        }

        /// <summary>
        /// Initializes the original grades and feedback dictionaries from the attempt's solutions.
        /// </summary>
        private void InitializeOriginalValues()
        {
            _originalGrades.Clear();
            _originalFeedback.Clear();
            _currentGrades.Clear();
            _currentFeedback.Clear();

            if (Attempt?.Solutions == null) return;

            foreach (var solution in Attempt.Solutions)
            {
                _originalGrades[solution.Id] = solution.ReceivedGrade;
                _originalFeedback[solution.Id] = solution.Feedback;
            }
        }

        /// <summary>
        /// Subscribe to real-time attempt updates for the current attempt
        /// </summary>
        private void SubscribeToRealTimeUpdates()
        {
            if (Attempt == null) return;

            _attemptUpdatedHandler = (updatedAttempt) =>
            {
                // Only process if this is our attempt
                if (updatedAttempt.Id != Attempt.Id) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Update the attempt with new data
                    Attempt = updatedAttempt;

                    // Update the grade and AI assessment for the currently selected question if it changed
                    if (SelectedQuestion != null)
                    {
                        var solution = Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion.Id);
                        if (solution != null)
                        {
                            if (Grade != solution.ReceivedGrade)
                            {
                                Grade = solution.ReceivedGrade;
                            }

                            // Update AI assessment if it changed
                            if (CurrentAiAssessment?.Id != solution.AiAssessment?.Id)
                            {
                                CurrentSolution = solution;
                                CurrentAiAssessment = solution.AiAssessment;
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Real-time: Attempt {Attempt.Id} updated with new grade data");
                });
            };

            _attemptsRepository.SubscribeUpdate(_attemptUpdatedHandler);

            // Subscribe to evaluation status updates
            _evaluationStatusHandler = (payload) =>
            {
                // Only process if this is our attempt
                if (payload.AttemptId != Attempt!.Id) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    switch (payload.Status)
                    {
                        case "EvaluationStarted":
                            EvaluationStatus = "Evaluating solution...";
                            break;
                        case "SystemGradingComplete":
                            EvaluationStatus = "System grading complete, AI assessment in progress...";
                            break;
                        case "AiAssessmentComplete":
                            EvaluationStatus = "AI assessment complete";
                            // Refresh the attempt to get the new AI assessment data
                            _ = RefreshAttemptDataAsync();
                            // Clear status after a delay
                            Task.Delay(3000).ContinueWith(_ =>
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    if (EvaluationStatus == "AI assessment complete")
                                        EvaluationStatus = null;
                                });
                            });
                            break;
                        case "EvaluationFailed":
                            EvaluationStatus = $"Evaluation failed: {payload.ErrorMessage}";
                            break;
                    }

                    System.Diagnostics.Debug.WriteLine($"Real-time: Evaluation status for Attempt {payload.AttemptId}: {payload.Status}");
                });
            };

            _attemptsRepository.SubscribeEvaluationStatus(_evaluationStatusHandler);
        }

        /// <summary>
        /// Refreshes the attempt data from the server to get updated AI assessments.
        /// </summary>
        private async Task RefreshAttemptDataAsync()
        {
            if (Attempt == null) return;

            try
            {
                var refreshedAttempt = await _attemptsRepository.GetAttemptById(Attempt.Id);
                if (refreshedAttempt != null)
                {
                    Attempt = refreshedAttempt;

                    // Update the current solution and AI assessment for the selected question
                    if (SelectedQuestion != null)
                    {
                        var solution = Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion.Id);
                        if (solution != null)
                        {
                            CurrentSolution = solution;
                            CurrentAiAssessment = solution.AiAssessment; // This will trigger OnPropertyChanged for suggested grade properties
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Refreshed attempt {Attempt.Id} with updated AI assessment data");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to refresh attempt data: {ex.Message}");
            }
        }

        /// <summary>
        /// Unsubscribe from real-time updates when leaving the view
        /// </summary>
        private void UnsubscribeFromUpdates()
        {
            if (_attemptUpdatedHandler != null)
            {
                _attemptsRepository.UnsubscribeUpdate(_attemptUpdatedHandler);
                _attemptUpdatedHandler = null;
            }
            if (_evaluationStatusHandler != null)
            {
                _attemptsRepository.UnsubscribeEvaluationStatus(_evaluationStatusHandler);
                _evaluationStatusHandler = null;
            }
        }

        public async Task ReturnToPreviousPage()
        {
            // Check if there are unsaved changes
            UpdateHasUnsavedChanges();
            if (HasUnsavedChanges)
            {
                var confirmed = await _uiService.ShowConfirmationAsync(
                    "Unsaved Changes",
                    "You have unsaved grade changes. Do you want to discard them?",
                    "Discard",
                    "Cancel");

                if (!confirmed)
                    return;
            }

            UnsubscribeFromUpdates();
            await _navigationService.GoToAsync("..");
        }

        /// <summary>
        /// Saves all changed grades and feedback for all solutions and navigates back.
        /// Only solutions with actually changed values are updated.
        /// Uses batch endpoint to send a single email notification.
        /// </summary>
        public async Task SaveAllGradesAndGoBack()
        {
            await ExecuteAsync(async () =>
            {
                // First save current question's values to tracking dictionaries
                if (SelectedQuestion != null && Attempt != null)
                {
                    var currentSol = Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion.Id);
                    if (currentSol != null)
                    {
                        _currentGrades[currentSol.Id] = Grade;
                        _currentFeedback[currentSol.Id] = Feedback;
                    }
                }

                // Collect all changed solutions for batch update
                var updates = new List<SolutionGradeUpdate>();

                foreach (var solution in Attempt!.Solutions)
                {
                    var originalGrade = _originalGrades.GetValueOrDefault(solution.Id);
                    var currentGrade = _currentGrades.GetValueOrDefault(solution.Id, solution.ReceivedGrade);
                    var originalFeedbackVal = _originalFeedback.GetValueOrDefault(solution.Id);
                    var currentFeedbackVal = _currentFeedback.GetValueOrDefault(solution.Id, solution.Feedback);

                    bool gradeChanged = currentGrade != originalGrade;
                    bool feedbackChanged = currentFeedbackVal != originalFeedbackVal;

                    if (gradeChanged || feedbackChanged)
                    {
                        // Find the question for this solution to get the question number
                        var question = Quiz?.Questions.Find(q => q.Id == solution.QuestionId);

                        updates.Add(new SolutionGradeUpdate
                        {
                            SolutionId = solution.Id,
                            QuestionNumber = question?.Order ?? 0,
                            TotalPoints = question?.Points ?? 0,
                            ReceivedGrade = currentGrade,
                            OldGrade = originalGrade,
                            Feedback = currentFeedbackVal,
                            OldFeedback = originalFeedbackVal,
                            EvaluatedBy = _authRepository.LoggedInUser?.FullName ?? "Instructor"
                        });
                    }
                }

                // If there are changes, send them all in a single batch request
                if (updates.Count > 0)
                {
                    try
                    {
                        var batchRequest = new BatchUpdateSolutionGradesRequest
                        {
                            AttemptId = Attempt.Id,
                            Updates = updates
                        };

                        await _attemptsRepository.BatchUpdateSolutionGrades(batchRequest);
                        System.Diagnostics.Debug.WriteLine($"Saved {updates.Count} grade/feedback updates in batch for attempt {Attempt.Id}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to save batch grades: {ex.Message}");
                        throw; // Re-throw to let ExecuteAsync handle it properly
                    }
                }

                HasUnsavedChanges = false;
                UnsubscribeFromUpdates();
                await _navigationService.GoToAsync("..");
            }, "Saving grades...");
        }

        public void NextQuestion()
        {
            if (SelectedQuestion!.Order + 1 <= Quiz!.Questions.Count)
            {
                SelectedQuestion = Quiz!.Questions.Find(q => q.Order == SelectedQuestion!.Order + 1);
                // Grade and Feedback are now set inside SelectedQuestion setter
            }
        }

        public void PreviousQuestion()
        {
            if (SelectedQuestion!.Order - 1 > 0)
            {
                SelectedQuestion = Quiz!.Questions.Find(q => q.Order == SelectedQuestion!.Order - 1);
                // Grade and Feedback are now set inside SelectedQuestion setter
            }
        }

        public void SpecificQuestion(Question question)
        {
            SelectedQuestion = question;
            // Grade and Feedback are now set inside SelectedQuestion setter
        }

        public async Task Run()
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

        [Obsolete("Grades are now saved only via SaveAllGradesAndGoBack")]
        private async Task SaveSolutionGradeAsync()
        {
            // This method is kept for backward compatibility but no longer used.
            // Grades are now tracked in memory and only saved when the user clicks "Save All Grades"
            await Task.CompletedTask;
        }

        [Obsolete("Grades are now saved only via SaveAllGradesAndGoBack")]
        public void SaveSolution()
        {
            // Keep for backward compatibility but should not be used for grading
        }

        private IExecutionRepository _executionRepository;
        private IAttemptsRepository _attemptsRepository;
        private INavigationService _navigationService;
        private IAuthenticationRepository _authRepository;
        private IUIService _uiService;

        public GradeAttemptVM(IExecutionRepository executionRepository, IAttemptsRepository attemptsRepository, INavigationService navigationService, IAuthenticationRepository authRepository, IUIService uiService)
        {
            _executionRepository = executionRepository;
            _attemptsRepository = attemptsRepository;
            _navigationService = navigationService;
            _authRepository = authRepository;
            _uiService = uiService;
        }
    }
}
