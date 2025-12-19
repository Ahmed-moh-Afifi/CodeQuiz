using CommunityToolkit.Maui.Views;
using CodeQuizDesktop.Viewmodels;
using CodeQuizDesktop.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CodeQuizDesktop.Repositories;

namespace CodeQuizDesktop.Views;

public partial class AddQuestionDialog : Popup<NewQuestionModel?>
{
    private NewQuestionModel? questionModel;
    private string? statement;
    private string editorCode = "";
    private string? programmingLanguage;
    private bool allowExecution = false;
    private bool showOutput = false;
    private bool showError = false;
    private bool allowIntellisense = false;
    private bool allowSignatureHelp = false;
    private ObservableCollection<TestCase> testCases = [];
    private float points = 0;
    private bool independentlyConfigured = false;



    public NewQuestionModel? QuestionModel
    {
        get => questionModel;
        set
        {
            questionModel = value;
            OnPropertyChanged();
        }
    }
    public string? Statement
    {
        get => statement;
        set
        {
            statement = value;
            OnPropertyChanged();
        }
    }
    public string EditorCode
    {
        get => editorCode;
        set
        {
            editorCode = value;
            OnPropertyChanged();
        }
    }
    public ObservableCollection<TestCase> TestCases
    {
        get => testCases;
        set
        {
            testCases = value;
            OnPropertyChanged();
        }
    }
    public int Order { get; set; } = -1;
    public float Points
    {
        get => points;
        set
        {
            points = value;
            OnPropertyChanged();
        }
    }

    // Configuration
    public string? ProgrammingLanguage
    {
        get
        {
            return programmingLanguage;
        }
        set
        {
            OnPropertyChanged();
            programmingLanguage = value;
        }
    }
    public bool AllowExecution
    {
        get => allowExecution;
        set
        {
            allowExecution = value;
            OnPropertyChanged();
        }
    }
    public bool ShowOutput
    {
        get => showOutput;
        set
        {
            showOutput = value;
            OnPropertyChanged();
        }
    }
    public bool ShowError
    {
        get => showError;
        set
        {
            showError = value;
            OnPropertyChanged();
        }
    }
    public bool AllowIntellisense
    {
        get => allowIntellisense;
        set
        {
            allowIntellisense = value;
            OnPropertyChanged();
        }
    }
    public bool AllowSignatureHelp
    {
        get => allowSignatureHelp;
        set
        {
            allowSignatureHelp = value;
            OnPropertyChanged();
        }
    }

    public bool IndependentlyConfigured
    {
        get
        {
            return independentlyConfigured;
        }
        set
        {
            independentlyConfigured = value;
            OnPropertyChanged();
        }
    }

    // Data Sources
    private List<string> programmingLanguages = ["Python"];
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

    //Commands
    public ICommand AddTestCaseCommand { get => new Command(AddTestCase); }
    public ICommand DeleteTestCaseCommand { get => new Command<TestCase>(DeleteTestCase); }

    public AddQuestionDialog(NewQuestionModel newQuestionModel = null)
    {
        InitializeComponent();
        BindingContext = this;
        QuestionModel = newQuestionModel;
        if (QuestionModel != null)
        {
            System.Diagnostics.Debug.WriteLine($"Statement: {QuestionModel.Statement}");
            Statement = QuestionModel.Statement;
            Points = QuestionModel.Points;
            EditorCode = QuestionModel.EditorCode;
            //TestCases = new ObservableCollection<TestCase>(QuestionModel.TestCases);
            TestCases = new ObservableCollection<TestCase>();
            this.Opened += async (s, e) =>
            {
                // Optional: A tiny delay to let the animation finish completely
                await Task.Delay(100);

                if (QuestionModel.TestCases != null)
                {
                    foreach (var tc in QuestionModel.TestCases)
                    {
                        // Add them one by one, just like clicking the button
                        TestCases.Add(tc);
                    }
                }
            };
            if (QuestionModel.QuestionConfiguration != null)
            {
                IndependentlyConfigured = true;
                ProgrammingLanguage = QuestionModel.QuestionConfiguration.Language;
                AllowExecution = QuestionModel.QuestionConfiguration.AllowExecution;
                ShowOutput = QuestionModel.QuestionConfiguration.ShowOutput;
                ShowError = QuestionModel.QuestionConfiguration.ShowError;
                AllowIntellisense = QuestionModel.QuestionConfiguration.AllowIntellisense;
                AllowSignatureHelp = QuestionModel.QuestionConfiguration.AllowSignatureHelp;
            }
            LoadProgrammingLanguages();
        }
        TestCases.CollectionChanged += (item, e) =>
        {
            for (int i = 0; i < TestCases.Count; i++)
            {
                TestCases[i].TestCaseNumber = i + 1;
            }
        };
    }

    public async void LoadProgrammingLanguages()
    {
        var executionRepository = MauiProgram.GetService<IExecutionRepository>();
        var languages = await executionRepository.GetSupportedLanguages();
        ProgrammingLanguages = languages.ToList();
    }

    public void AddTestCase()
    {
        TestCases.Add(new TestCase() { TestCaseNumber = -1, Input = [], ExpectedOutput = "" });
    }

    public void DeleteTestCase(TestCase tc)
    {
        TestCases.Remove(tc);
    }

    /// <summary>
    /// Validates the current user input and returns a list of error messages describing any detected issues.
    /// </summary>
    /// <remarks>Validation includes checking that the question statement is not empty, that a programming
    /// language is selected when required, and that all test cases are valid. This method does not throw exceptions for
    /// validation failures; all errors are reported in the returned list.</remarks>
    /// <returns>A list of strings containing error messages for each validation failure. The list is empty if no errors are
    /// found.</returns>
    public List<string> ValidateUserInput()
    {
        List<string> errorMessages = [];
        if (string.IsNullOrEmpty(Statement?.Trim()))
        {
            // Title required error
            errorMessages.Add("Question statement cannot be empty");
        }

        if (IndependentlyConfigured && string.IsNullOrEmpty(ProgrammingLanguage?.Trim()))
        {
            // Programming language not selected error
            errorMessages.Add("Programming language not selected");
        }

        errorMessages.AddRange(ValidateTestCases());

        return errorMessages;
    }

    /// <summary>
    /// Validates the collection of test cases to ensure that no two test cases have identical input sequences.
    /// </summary>
    /// <remarks>This method checks for duplicate input sequences among all test cases. Each error message
    /// identifies the test case number with a duplicate input. Use this method to verify the integrity of test case
    /// inputs before adding tests.</remarks>
    /// <returns>A list of error messages describing any test cases that share the same input. The list is empty if all test
    /// cases have unique inputs.</returns>
    public List<string> ValidateTestCases()
    {
        List<string> errors = [];
        foreach (var tc in TestCases)
        {
            var inputString = string.Join('\n', tc.Input);
            int count = 0;
            foreach (var tc2 in TestCases)
            {
                var inputString2 = string.Join('\n', tc2.Input);
                if (inputString == inputString2)
                    count++;
            }

            if (count > 1)
                errors.Add($"Same input cannot be used in more than one testcase. Test number {tc.TestCaseNumber}");
        }

        return errors;
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        var errors = ValidateUserInput();
        if (errors.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine(string.Join('\n', errors));
            return;
        }

        var newQuestion = new NewQuestionModel
        {
            Statement = Statement!,
            EditorCode = EditorCode!,
            TestCases = TestCases.ToList(),
            Points = Points,
            Order = Order,
            QuestionConfiguration = IndependentlyConfigured ? new QuestionConfiguration
            {
                Language = ProgrammingLanguage!,
                AllowExecution = AllowExecution,
                ShowOutput = ShowOutput,
                ShowError = ShowError,
                AllowIntellisense = AllowIntellisense,
                AllowSignatureHelp = AllowSignatureHelp
            } : null
        };

        await CloseAsync(newQuestion);
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }
}