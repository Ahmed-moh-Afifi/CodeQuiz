using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
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
                OnPropertyChanged(nameof(IsCreatingNew));
            }
        }

        public bool IsCreatingNew => EditedQuizId == null;

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

        private SupportedLanguage? selectedSupportedLanguage;
        public SupportedLanguage? SelectedSupportedLanguage
        {
            get => selectedSupportedLanguage ?? ProgrammingLanguages.FirstOrDefault(l => l.Name == ProgrammingLanguage);
            set
            {
                selectedSupportedLanguage = value;
                if (value != null)
                {
                    ProgrammingLanguage = value.Name;
                }
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
                OnPropertyChanged(nameof(SelectedSupportedLanguage));
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
        private List<SupportedLanguage> programmingLanguages = [];
        public List<SupportedLanguage> ProgrammingLanguages
        {
            get
            {
                return programmingLanguages;
            }
            set
            {
                programmingLanguages = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedSupportedLanguage));
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
        public ICommand AddQuestionCommand { get => new Command(async () => await AddQuestion()); }
        public ICommand ReturnCommand { get => new Command(async () => await ReturnToPreviousPage()); }
        public ICommand PublishCommand { get => new Command(async () => await CreateAndPublishQuizAsync()); }
        public ICommand DeleteQuestionCommand { get => new Command<NewQuestionModel>(async (q) => await DeleteQuestionAsync(q)); }
        public ICommand EditQuestionCommand { get => new Command<NewQuestionModel>(async (q) => await EditQuestion(q)); }

        private readonly IQuizDialogService quizDialogService;
        private readonly INavigationService navigationService;
        private readonly IAuthenticationRepository authenticationRepository;
        private readonly IQuizzesRepository quizzesRepository;
        private readonly IExecutionRepository executionRepository;
        private readonly IUIService uiService;

        public async Task LoadProgrammingLanguages()
        {
            var languages = await executionRepository.GetSupportedLanguages();
            ProgrammingLanguages = languages.ToList();

            // Select language if editing
            if (programmingLanguage != null)
            {
                // This implies UI binding to SelectedItem expects a SupportedLanguage, but ProgrammingLanguage is string
                // We should probably keep ProgrammingLanguage as string for compatibility with Model, but the UI ComboBox needs to bind to something
            }
        }

        public async Task ReturnToPreviousPage()
        {
            // Show confirmation if there are unsaved changes
            if (QuestionModels.Count > 0 || !string.IsNullOrEmpty(QuizTitle))
            {
                var confirmed = await uiService.ShowConfirmationAsync(
                    "Discard Changes?",
                    "You have unsaved changes. Are you sure you want to go back?",
                    "Discard",
                    "Stay");

                if (!confirmed)
                    return;
            }

            await navigationService.GoToAsync("..");
        }

        public async Task AddQuestion()
        {
            var result = await quizDialogService.ShowAddQuestionDialogAsync();

            if (result != null)
            {
                QuestionModels.Add(result);
            }
        }

        public async Task DeleteQuestionAsync(NewQuestionModel q)
        {
            // Show confirmation dialog before deleting
            var confirmed = await uiService.ShowDestructiveConfirmationAsync(
                "Delete Question",
                $"Are you sure you want to delete Question {q.Order}? This action cannot be undone.",
                "Delete",
                "Cancel");

            if (confirmed)
            {
                QuestionModels.Remove(q);
            }
        }

        public async Task EditQuestion(NewQuestionModel newQuestionModel)
        {
            var result = await quizDialogService.ShowEditQuestionDialogAsync(newQuestionModel);

            if (result is NewQuestionModel updatedModel)
            {
                System.Diagnostics.Debug.WriteLine($"Question updated successfully!");
                int index = QuestionModels.IndexOf(newQuestionModel);
                if (index >= 0)
                {
                    QuestionModels[index] = updatedModel;
                }
            }
        }

        public List<string> ValidateUserInput()
        {
            List<string> errorMessages = [];
            if (string.IsNullOrEmpty(QuizTitle?.Trim()))
            {
                errorMessages.Add("Quiz title cannot be empty");
            }

            if (!int.TryParse(QuizDurationInMinutes!, out _))
            {
                errorMessages.Add("Invalid duration value");
            }

            if (AvailableToDateTime <= AvailableFromDateTime)
            {
                errorMessages.Add("Quiz cannot be closed before it is opened (check availability values)");
            }

            if (int.TryParse(QuizDurationInMinutes!, out int minsDuration) && AvailableToDateTime.Subtract(AvailableFromDateTime).TotalMinutes < minsDuration)
            {
                errorMessages.Add("Quiz availability period cannot be shorter than its duration");
            }

            if (string.IsNullOrEmpty(ProgrammingLanguage?.Trim()))
            {
                errorMessages.Add("Programming language not selected");
            }

            if (QuestionModels.Count == 0)
            {
                errorMessages.Add("Cannot create quiz without questions");
            }

            return errorMessages;
        }

        public async Task CreateAndPublishQuizAsync()
        {
            var errors = ValidateUserInput();
            if (errors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Missing or invalid input");
                await uiService.ShowErrorAsync(string.Join("\n", errors), "Invalid Input");
                return;
            }

            var confirmed = await uiService.ShowConfirmationAsync(
                "Confirm Publish",
                $"Are you sure you want to {(QuizModel == null && EditedQuizId == null ? "publish" : "update")} this quiz?",
                (QuizModel == null && EditedQuizId == null ? "Publish" : "Update"),
                "Cancel");

            if (!confirmed)
                return;

            var loadingMessage = QuizModel == null && EditedQuizId == null ? "Publishing quiz..." : "Updating quiz...";

            var minsDuration = int.Parse(QuizDurationInMinutes!);

            // Convert local times to UTC before sending to backend
            var newQuizModel = new NewQuizModel()
            {
                Title = QuizTitle!,
                Duration = TimeSpan.FromMinutes(minsDuration),
                StartDate = AvailableFromDateTime.ToUniversalTime(),
                EndDate = AvailableToDateTime.ToUniversalTime(),
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
            await ExecuteAsync(async () =>
            {
                if (QuizModel == null && EditedQuizId == null)
                {
                    var quiz = await quizzesRepository.CreateQuiz(newQuizModel);
                }
                else if (QuizModel != null && EditedQuizId != null)
                {
                    var quiz = await quizzesRepository.UpdateQuiz((int)EditedQuizId, newQuizModel);
                }

                await navigationService.GoToAsync("..");
            }, loadingMessage);
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
                AllowSignatureHelp = receivedQuizModel.GlobalQuestionConfiguration.AllowSignatureHelp;
            }
        }

        public CreateQuizVM(IQuizDialogService quizDialogService, INavigationService navigationService, IAuthenticationRepository authenticationRepository, IQuizzesRepository quizzesRepository, IExecutionRepository executionRepository, IUIService uiService)
        {
            this.quizDialogService = quizDialogService;
            this.navigationService = navigationService;
            this.authenticationRepository = authenticationRepository;
            this.quizzesRepository = quizzesRepository;
            this.executionRepository = executionRepository;
            this.uiService = uiService;
            QuestionModels = [];
            _ = LoadProgrammingLanguages();
        }
    }
}
