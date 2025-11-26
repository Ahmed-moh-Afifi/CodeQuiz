using CommunityToolkit.Maui.Views;
using CodeQuizDesktop.Viewmodels;
using CodeQuizDesktop.Models;

namespace CodeQuizDesktop.Views;

public partial class AddQuestionDialog : Popup<NewQuestionModel?>
{
    private string statement;
    private string editorCode;
    private string language;
    private bool allowExecution;
    private bool showOutput;
    private bool showError;
    private List<TestCase> testCases;
    private float points;

    public string Statement
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
    public List<TestCase> TestCases
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
    public string Language
    {
        get => language;
        set
        {
            language = value;
            OnPropertyChanged();
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

    public AddQuestionDialog()
	{
		InitializeComponent();
		BindingContext = this;
	}

	private async void OnAddButtonClicked(object sender, EventArgs e)
	{
        var newQuestion = new NewQuestionModel
        {
            Statement = Statement,
            EditorCode = EditorCode,
            TestCases = TestCases,
            Points = Points,
            Order = Order,
            QuestionConfiguration = new QuestionConfiguration
            {
                Language = Language,
                AllowExecution = AllowExecution,
                ShowOutput = ShowOutput,
                ShowError = ShowError
            }
        };

        await CloseAsync(newQuestion);
    }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
	{
		await CloseAsync(null);
    }
}