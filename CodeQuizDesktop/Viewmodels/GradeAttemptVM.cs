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
                // Update Grade when question changes
                Grade = Attempt!.Solutions.Find(s => s.QuestionId == value.Id)?.ReceivedGrade;

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

        private float? grade;

        public float? Grade
        {
            get { return grade; }
            set
            {
                grade = value;
                OnPropertyChanged();
            }
        }
        
        private Action<ExaminerAttempt>? _attemptUpdatedHandler;

        //Commands
        public ICommand ReturnCommand { get => new Command(async () => await ReturnToPreviousPage()); }
        public ICommand NextQuestionCommand { get => new Command(NextQuestion); }
        public ICommand PreviousQuestionCommand { get => new Command(PreviousQuestion); }
        public ICommand SpecificQuestionCommand { get => new Command<Question>(SpecificQuestion); }
        public ICommand RunCommand { get => new Command(async () => await Run()); }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("attempt") && query["attempt"] is ExaminerAttempt receivedAttempt)
            {
                Attempt = receivedAttempt;
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
                    
                    // Update the grade for the currently selected question if it changed
                    if (SelectedQuestion != null)
                    {
                        var solution = Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion.Id);
                        if (solution != null && Grade != solution.ReceivedGrade)
                        {
                            Grade = solution.ReceivedGrade;
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Real-time: Attempt {Attempt.Id} updated with new grade data");
                });
            };
            
            _attemptsRepository.SubscribeUpdate(_attemptUpdatedHandler);
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
        }
        
        public async Task ReturnToPreviousPage()
        {
            await SaveSolutionGradeAsync();
            UnsubscribeFromUpdates();
            await _navigationService.GoToAsync("..");
        }

        public async void NextQuestion()
        {
            await SaveSolutionGradeAsync();
            if (SelectedQuestion!.Order + 1 <= Quiz!.Questions.Count)
            {
                SelectedQuestion = Quiz!.Questions.Find(q => q.Order == SelectedQuestion!.Order + 1);
                // Grade is now set inside SelectedQuestion setter
            }
        }

        public async void PreviousQuestion()
        {
            await SaveSolutionGradeAsync();
            if (SelectedQuestion!.Order - 1 > 0)
            {
                SelectedQuestion = Quiz!.Questions.Find(q => q.Order == SelectedQuestion!.Order - 1);
                // Grade is now set inside SelectedQuestion setter
            }
        }

        public async void SpecificQuestion(Question question)
        {
            await SaveSolutionGradeAsync();
            SelectedQuestion = question;
            // Grade is now set inside SelectedQuestion setter
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

        /// <summary>
        /// Saves the grade for the current solution using the dedicated grading endpoint.
        /// </summary>
        private async Task SaveSolutionGradeAsync()
        {
            if (Grade.HasValue)
            {
                var solution = Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!;
                var request = new UpdateSolutionGradeRequest
                {
                    SolutionId = solution.Id,
                    ReceivedGrade = Grade.Value,
                    EvaluatedBy = _authRepository.LoggedInUser?.FullName ?? "Instructor"
                };
                
                try
                {
                    await _attemptsRepository.UpdateSolutionGrade(request);
                    // Update local state
                    solution.ReceivedGrade = Grade.Value;
                    solution.EvaluatedBy = request.EvaluatedBy;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to save grade: {ex.Message}");
                }
            }
        }

        [Obsolete("Use SaveSolutionGradeAsync instead - grades should be saved via the dedicated grading endpoint")]
        public void SaveSolution()
        {
            // Keep for backward compatibility but should not be used for grading
            _ = SaveSolutionGradeAsync();
        }

        private IExecutionRepository _executionRepository;
        private IAttemptsRepository _attemptsRepository;
        private INavigationService _navigationService;
        private IAuthenticationRepository _authRepository;
        
        public GradeAttemptVM(IExecutionRepository executionRepository, IAttemptsRepository attemptsRepository, INavigationService navigationService, IAuthenticationRepository authRepository)
        {
            _executionRepository = executionRepository;
            _attemptsRepository = attemptsRepository;
            _navigationService = navigationService;
            _authRepository = authRepository;
        }
    }
}
