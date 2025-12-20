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
                //System.Diagnostics.Debug.WriteLine($"Attempt ID: {receivedAttempt.Id}");
            }
            if (query.ContainsKey("quiz") && query["quiz"] is ExaminerQuiz receivedQuiz)
            {
                Quiz = receivedQuiz;
                SelectedQuestion = Quiz!.Questions.Find(q => q.Order == 1);
                Grade = Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.ReceivedGrade;
                System.Diagnostics.Debug.WriteLine($"Quiz ID: {receivedQuiz.Id}");
            }
        }
        public async Task ReturnToPreviousPage()
        {
            SaveSolution();
            await _navigationService.GoToAsync("..");
        }

        public void NextQuestion()
        {
            SaveSolution();
            if (SelectedQuestion!.Order + 1 <= Quiz!.Questions.Count)
            {
                SelectedQuestion = Quiz!.Questions.Find(q => q.Order == SelectedQuestion!.Order + 1);
                Grade = Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.ReceivedGrade;
            }
        }

        public void PreviousQuestion()
        {
            SaveSolution();
            if (SelectedQuestion!.Order - 1 > 0)
            {
                SelectedQuestion = Quiz!.Questions.Find(q => q.Order == SelectedQuestion!.Order - 1);
                Grade = Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.ReceivedGrade;
            }
        }

        public void SpecificQuestion(Question question)
        {
            SaveSolution();
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

        public void SaveSolution()
        {
            Attempt!.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!.ReceivedGrade = Grade;
            _attemptsRepository.UpdateSolution(Attempt.Solutions.Find(s => s.QuestionId == SelectedQuestion!.Id)!);
        }

        private IExecutionRepository _executionRepository;
        private IAttemptsRepository _attemptsRepository;
        private INavigationService _navigationService;
        public GradeAttemptVM(IExecutionRepository executionRepository, IAttemptsRepository attemptsRepository, INavigationService navigationService)
        {
            _executionRepository = executionRepository;
            _attemptsRepository = attemptsRepository;
            _navigationService = navigationService;
        }
    }
}
