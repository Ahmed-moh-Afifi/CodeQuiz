using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class CreateQuizVM : BaseViewModel, IQueryAttributable
    {
        // UI properties
        private NewQuizModel? quizModel;
        public NewQuizModel? QuizModel
        {
            get => quizModel;
            set
            {
                quizModel = value;
                OnPropertyChanged();
            }
        }
        private int? editedQuizId;
        public int? EditedQuizId
        {
            get => editedQuizId;
            set
            {
                editedQuizId = value;
                OnPropertyChanged();
            }
        }

        private string header = "New Quiz";
        public string Header
        {
            get
            {
                return header;
            }
            set
            {
                header = value;
                OnPropertyChanged();
            }
        }

        private string? quizTitle;
        public string? QuizTitle
        {
            get
            {
                return quizTitle;
            }
            set
            {
                quizTitle = value;
                OnPropertyChanged();
            }
        }

        private string? quizDurationInMinutes;
        public string? QuizDurationInMinutes
        {
            get
            {
                return quizDurationInMinutes;
            }
            set
            {
                quizDurationInMinutes = value;
                OnPropertyChanged();
            }
        }

        private DateTime availableFromDate = DateTime.Now.AddDays(1);
        public DateTime AvailableFromDate
        {
            get
            {
                return availableFromDate;
            }
            set
            {
                availableFromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime availableToDate = DateTime.Now.AddDays(5);
        public DateTime AvailableToDate
        {
            get
            {
                return availableToDate;
            }
            set
            {
                availableToDate = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan availableFromTime;
        public TimeSpan AvailableFromTime
        {
            get
            {
                return availableFromTime;
            }
            set
            {
                availableFromTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan availableToTime;
        public TimeSpan AvailableToTime
        {
            get
            {
                return availableToTime;
            }
            set
            {
                availableToTime = value;
                OnPropertyChanged();
            }
        }

        private string? programmingLanguage;
        public string? ProgrammingLanguage
        {
            get
            {
                return programmingLanguage;
            }
            set
            {
                programmingLanguage = value;
                OnPropertyChanged();
            }
        }

        private bool showOutput = false;
        public bool ShowOutput
        {
            get
            {
                return showOutput;
            }
            set
            {
                showOutput = value;
                OnPropertyChanged();
            }
        }

        private bool showErrors = false;
        public bool ShowErrors
        {
            get
            {
                return showErrors;
            }
            set
            {
                showErrors = value;
                OnPropertyChanged();
            }
        }

        private bool allowIntellisense = false;
        public bool AllowIntellisense
        {
            get
            {
                return allowIntellisense;
            }
            set
            {
                allowIntellisense = value;
                OnPropertyChanged();
            }
        }

        private bool allowSignatureHelp = false;
        public bool AllowSignatureHelp
        {
            get
            {
                return allowSignatureHelp;
            }
            set
            {
                allowSignatureHelp = value;
                OnPropertyChanged();
            }
        }

        private bool allowMultipleAttempts = false;
        public bool AllowMultipleAttempts
        {
            get
            {
                return allowMultipleAttempts;
            }
            set
            {
                allowMultipleAttempts = value;
                OnPropertyChanged();
            }
        }

        private bool allowExecution = false;
        public bool AllowExecution
        {
            get
            {
                return allowExecution;
            }
            set
            {
                allowExecution = value;
                OnPropertyChanged();
            }

        }

        private ObservableCollection<NewQuestionModel> questionModels = [];
        public ObservableCollection<NewQuestionModel> QuestionModels
        {
            get
            {
                return questionModels;
            }
            set
            {
                if (questionModels != null)
                {
                    questionModels.CollectionChanged -= QuestionModels_CollectionChanged;
                }
                questionModels = value;
                if (questionModels != null)
                {
                    questionModels.CollectionChanged += QuestionModels_CollectionChanged;
                }
                OnPropertyChanged();
            }
        }

        private void QuestionModels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < QuestionModels.Count; i++)
            {
                QuestionModels[i].Order = i + 1;
            }
        }

        // Data Sources
        private List<string> programmingLanguages = ["CSharp"];
        public List<string> ProgrammingLanguages
        {
            get
            {
                return programmingLanguages;
            }
            set
            {
                programmingLanguages = value;
                OnPropertyChanged();
            }
        }

        // Conversion getters
        public DateTime AvailableFromDateTime
        {
            get
            {
                return new
                    (AvailableFromDate.Year,
                    AvailableFromDate.Month,
                    AvailableFromDate.Day,
                    AvailableFromTime.Hours,
                    AvailableFromTime.Minutes,
                    AvailableFromTime.Seconds);
            }
        }

        public DateTime AvailableToDateTime
        {
            get
            {
                return new
                    (AvailableToDate.Year,
                    AvailableToDate.Month,
                    AvailableToDate.Day,
                    AvailableToTime.Hours,
                    AvailableToTime.Minutes,
                    AvailableToTime.Seconds);
            }
        }

        // Button Commands, mapped to viewmodel methods
        public ICommand AddQuestionCommand { get => new Command(AddQuestion); }
        public ICommand ReturnCommand { get => new Command(ReturnToPreviousPage); }
        public ICommand PublishCommand { get => new Command(CreateAndPublishQuiz); }
        public ICommand DeleteQuestionCommand { get => new Command<NewQuestionModel>(DeleteQuestion); }
        public ICommand EditQuestionCommand { get => new Command<NewQuestionModel>(EditQuestion); }

        private readonly IPopupService popupService;
        private readonly IAuthenticationRepository authenticationRepository;
        private readonly IQuizzesRepository quizzesRepository;

        private async void ReturnToPreviousPage()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async void AddQuestion()
        {
            var result = await popupService.ShowPopupAsync<AddQuestionDialog, NewQuestionModel?>(Shell.Current, new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(20),
                    StrokeThickness = 0
                }
            });



            if (result.Result != null)
            {
                QuestionModels.Add(result.Result);
            }
            //await Application.Current!.MainPage!.DisplayPromptAsync("Result", $"You selected: {result}", "OK");
        }

        public void DeleteQuestion(NewQuestionModel q)
        {
            QuestionModels.Remove(q);
        }

        public async void EditQuestion(NewQuestionModel newQuestionModel)
        {
            // Create the instance manually and pass the parameter
            var popup = new AddQuestionDialog(newQuestionModel);

            // Show it using Shell.Current.CurrentPage
            var result = await Shell.Current.CurrentPage.ShowPopupAsync<NewQuestionModel?>(popup);

            if (result.Result is NewQuestionModel updatedModel)
            {
                System.Diagnostics.Debug.WriteLine($"Question updated successfully!");
                int index = QuestionModels.IndexOf(newQuestionModel);
                if (index >= 0)
                {
                    QuestionModels[index] = updatedModel;
                }
            }
        }

        /// <summary>
        /// Validates the current user input for quiz configuration and returns a list of error messages describing any
        /// validation failures.
        /// </summary>
        /// <remarks>Validation checks include ensuring the quiz title and programming language are
        /// specified, the duration is a valid number, the availability period is valid, and the quiz duration does not
        /// exceed the availability window. This method does not throw exceptions for invalid input; all issues are
        /// reported in the returned list.</remarks>
        /// <returns>A list of strings containing error messages for each validation issue found. The list is empty if all user
        /// input is valid.</returns>
        public List<string> ValidateUserInput()
        {
            List<string> errorMessages = [];
            if (string.IsNullOrEmpty(QuizTitle?.Trim()))
            {
                // Title required error
                errorMessages.Add("Quiz title cannot be empty");
            }

            if (!int.TryParse(QuizDurationInMinutes!, out _))
            {
                // Invalid duration error (not numeric)
                errorMessages.Add("Invalid duration value");
            }

            if (AvailableToDateTime <= AvailableFromDateTime)
            {
                // Quiz closes before it opens error
                errorMessages.Add("Quiz cannot be closed before it is opened (check availability values)");
            }

            if (int.TryParse(QuizDurationInMinutes!, out int minsDuration) && AvailableToDateTime.Subtract(AvailableFromDateTime).TotalMinutes < minsDuration)
            {
                // Quiz availability period cannot be shorter than its duration error
                errorMessages.Add("Quiz availability period cannot be shorter than its duration");
            }

            if (string.IsNullOrEmpty(ProgrammingLanguage?.Trim()))
            {
                // Programming language not selected error
                errorMessages.Add("Programming language not selected");
            }

            if (QuestionModels.Count == 0)
            {
                errorMessages.Add("Cannot create quiz without questions");
            }

            return errorMessages;
        }

        public async void CreateAndPublishQuiz()
        {
            var errors = ValidateUserInput();
            if (errors.Count > 0)
            {
                // Errors exist...
                System.Diagnostics.Debug.WriteLine("Missing or invalid input");
                return;
            }

            var minsDuration = int.Parse(QuizDurationInMinutes!);
            var newQuizModel = new NewQuizModel()
            {
                Title = QuizTitle!,
                Duration = TimeSpan.FromMinutes(minsDuration),
                StartDate = AvailableFromDateTime,
                EndDate = AvailableToDateTime,
                AllowMultipleAttempts = AllowMultipleAttempts,
                ExaminerId = authenticationRepository.LoggedInUser!.Id,
                GlobalQuestionConfiguration = new()
                {
                    Language = ProgrammingLanguage!,
                    AllowExecution = AllowExecution,
                    ShowError = ShowErrors,
                    ShowOutput = ShowOutput,
                    AllowIntellisense = AllowIntellisense,
                    AllowSignatureHelp = AllowSignatureHelp
                },
                Questions = QuestionModels.ToList()
            };
            if (QuizModel == null && EditedQuizId == null)
            {
                var quiz = await quizzesRepository.CreateQuiz(newQuizModel);
            }
            else if (QuizModel != null && EditedQuizId != null)
            {
                var quiz = await quizzesRepository.UpdateQuiz((int)EditedQuizId, newQuizModel);
            }

            ReturnToPreviousPage();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {

            if (query.ContainsKey("quizModel") && query["quizModel"] is NewQuizModel receivedQuizModel && query.ContainsKey("id") && query["id"] is int id)
            {
                EditedQuizId = id;
                QuizModel = receivedQuizModel;
                Header = "Edit Quiz";
                QuizTitle = receivedQuizModel.Title;
                AllowMultipleAttempts = receivedQuizModel.AllowMultipleAttempts;
                QuizDurationInMinutes = ((int)receivedQuizModel.Duration.TotalMinutes).ToString();
                AvailableFromDate = receivedQuizModel.StartDate;
                AvailableFromTime = new TimeSpan(receivedQuizModel.StartDate.Hour, receivedQuizModel.StartDate.Minute, receivedQuizModel.StartDate.Second);
                AvailableToDate = receivedQuizModel.EndDate;
                AvailableToTime = new TimeSpan(receivedQuizModel.EndDate.Hour, receivedQuizModel.EndDate.Minute, receivedQuizModel.EndDate.Second);
                ProgrammingLanguage = receivedQuizModel.GlobalQuestionConfiguration.Language;
                AllowExecution = receivedQuizModel.GlobalQuestionConfiguration.AllowExecution;
                ShowOutput = receivedQuizModel.GlobalQuestionConfiguration.ShowOutput;
                ShowErrors = receivedQuizModel.GlobalQuestionConfiguration.ShowError;
                QuestionModels = new ObservableCollection<NewQuestionModel>(receivedQuizModel.Questions);
                AllowIntellisense = receivedQuizModel.GlobalQuestionConfiguration.AllowIntellisense;
                allowSignatureHelp = receivedQuizModel.GlobalQuestionConfiguration.AllowSignatureHelp;
            }
        }

        public CreateQuizVM(IPopupService popupService, IAuthenticationRepository authenticationRepository, IQuizzesRepository quizzesRepository)
        {
            this.popupService = popupService;
            this.authenticationRepository = authenticationRepository;
            this.quizzesRepository = quizzesRepository;

            QuestionModels = [];
        }
    }

}
