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
    public class ExamineeReviewQuizVM : BaseViewModel, IQueryAttributable
    {
        private ExamineeAttempt attempt;
        public ExamineeAttempt Attempt
        {
            get { return attempt; }
            set
            {
                attempt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowAiFeedback));
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

                // Update current solution and AI assessment
                UpdateCurrentSolution();

                OnPropertyChanged();
            }
        }

        #region AI Assessment Properties

        /// <summary>
        /// Whether to show AI feedback based on instructor setting.
        /// </summary>
        public bool ShowAiFeedback => Attempt?.Quiz?.ShowAiFeedbackToStudents == true;

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
                OnPropertyChanged(nameof(HasAiAssessment));
                OnPropertyChanged(nameof(AiValidityText));
                OnPropertyChanged(nameof(AiConfidenceText));
                OnPropertyChanged(nameof(PassedTestCasesText));
                OnPropertyChanged(nameof(HasEvaluationResults));
                OnPropertyChanged(nameof(HasInstructorFeedback));
                OnPropertyChanged(nameof(InstructorFeedbackAuthor));
            }
        }

        /// <summary>
        /// Whether the current solution has an AI assessment.
        /// </summary>
        public bool HasAiAssessment => CurrentSolution?.AiAssessment != null && ShowAiFeedback;

        /// <summary>
        /// Whether the current solution has instructor feedback.
        /// </summary>
        public bool HasInstructorFeedback => !string.IsNullOrWhiteSpace(CurrentSolution?.Feedback);

        /// <summary>
        /// The name of the instructor who provided the feedback.
        /// Returns the EvaluatedBy value if available, otherwise "Instructor".
        /// </summary>
        public string InstructorFeedbackAuthor => CurrentSolution?.EvaluatedBy ?? "Instructor";

        /// <summary>
        /// Whether the current solution has evaluation results.
        /// </summary>
        public bool HasEvaluationResults => CurrentSolution?.EvaluationResults?.Any() == true;

        /// <summary>
        /// User-friendly text for AI validity status.
        /// </summary>
        public string AiValidityText => CurrentSolution?.AiAssessment?.IsValid == true
            ? "Valid Solution"
            : "Solution Needs Review";

        /// <summary>
        /// User-friendly text for AI confidence score.
        /// </summary>
        public string AiConfidenceText => CurrentSolution?.AiAssessment != null
            ? $"{(CurrentSolution.AiAssessment.ConfidenceScore * 100):F0}% confidence"
            : "";

        /// <summary>
        /// Text showing passed vs total test cases.
        /// </summary>
        public string PassedTestCasesText
        {
            get
            {
                if (CurrentSolution?.EvaluationResults == null || !CurrentSolution.EvaluationResults.Any())
                    return "";

                var passed = CurrentSolution.EvaluationResults.Count(r => r.IsSuccessful);
                var total = CurrentSolution.EvaluationResults.Count;
                return $"{passed}/{total} test cases passed";
            }
        }

        /// <summary>
        /// Updates the current solution when the selected question changes.
        /// </summary>
        private void UpdateCurrentSolution()
        {
            if (SelectedQuestion == null || Attempt == null)
            {
                CurrentSolution = null;
                return;
            }

            CurrentSolution = Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion.Id);
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
                grade = value;
                OnPropertyChanged();
            }
        }

        //Commands
        public ICommand ReturnCommand { get => new Command(async () => await ReturnToPreviousPage()); }
        public ICommand NextQuestionCommand { get => new Command(NextQuestion); }
        public ICommand PreviousQuestionCommand { get => new Command(PreviousQuestion); }
        public ICommand SpecificQuestionCommand { get => new Command<Question>(SpecificQuestion); }
        public ICommand RunCommand { get => new Command(async () => await Run()); }
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("attempt") && query["attempt"] is ExamineeAttempt receivedAttempt)
            {
                Attempt = receivedAttempt;
                SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == 1);
                // Grade is now set inside SelectedQuestion setter
                System.Diagnostics.Debug.WriteLine($"Clicked: {Attempt.Quiz.Title}");
            }
        }
        public async Task ReturnToPreviousPage()
        {
            await _navigationService.GoToAsync("..");
        }

        public void NextQuestion()
        {
            if (SelectedQuestion!.Order + 1 <= Attempt!.Quiz.Questions.Count)
            {
                SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == SelectedQuestion!.Order + 1);
                Grade = Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.ReceivedGrade;
            }
        }

        public void PreviousQuestion()
        {
            if (SelectedQuestion!.Order - 1 > 0)
            {
                SelectedQuestion = Attempt!.Quiz.Questions.Find(q => q.Order == SelectedQuestion!.Order - 1);
                Grade = Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.ReceivedGrade;
            }
        }

        public void SpecificQuestion(Question question)
        {
            SelectedQuestion = question;
            Grade = Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.ReceivedGrade;
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

        private IExecutionRepository _executionRepository;
        private INavigationService _navigationService;

        public ExamineeReviewQuizVM(IExecutionRepository executionRepository, INavigationService navigationService)
        {
            _executionRepository = executionRepository;
            _navigationService = navigationService;
        }
    }
}
