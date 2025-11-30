using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Controls;

public partial class CodeEditor : ContentView
{
    private readonly IExecutionRepository executionRepository = MauiProgram.GetService<IExecutionRepository>();
    private TaskCompletionSource<string>? _getCodeTcs;
    private bool _isEditorReady = false;
    private string? _pendingCode;
    private string? _pendingLanguage;

    #region Bindable Properties

    /// <summary>
    /// The programming language for the editor (e.g., "javascript", "csharp", "python")
    /// </summary>
    public static readonly BindableProperty LanguageProperty = BindableProperty.Create(
        nameof(Language),
        typeof(string),
        typeof(CodeEditor),
        "csharp",
        propertyChanged: OnLanguageChanged);

    public string Language
    {
        get => (string)GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value.ToLower());
    }

    /// <summary>
    /// Whether code execution is allowed (affects which editor is loaded)
    /// </summary>
    public static readonly BindableProperty AllowExecutionProperty = BindableProperty.Create(
        nameof(AllowExecution),
        typeof(bool),
        typeof(CodeEditor),
        true,
        propertyChanged: OnEditorConfigChanged);

    public bool AllowExecution
    {
        get => (bool)GetValue(AllowExecutionProperty);
        set => SetValue(AllowExecutionProperty, value);
    }

    /// <summary>
    /// Whether to show output (affects editor type selection)
    /// </summary>
    public static readonly BindableProperty ShowOutputProperty = BindableProperty.Create(
        nameof(ShowOutput),
        typeof(bool),
        typeof(CodeEditor),
        true,
        propertyChanged: OnEditorConfigChanged);

    public bool ShowOutput
    {
        get => (bool)GetValue(ShowOutputProperty);
        set => SetValue(ShowOutputProperty, value);
    }

    /// <summary>
    /// Whether to show errors (affects editor type selection)
    /// </summary>
    public static readonly BindableProperty ShowErrorProperty = BindableProperty.Create(
        nameof(ShowError),
        typeof(bool),
        typeof(CodeEditor),
        true,
        propertyChanged: OnEditorConfigChanged);

    public bool ShowError
    {
        get => (bool)GetValue(ShowErrorProperty);
        set => SetValue(ShowErrorProperty, value);
    }

    /// <summary>
    /// Whether the editor is read-only
    /// </summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(CodeEditor),
        false,
        propertyChanged: OnEditorConfigChanged);

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// The type of editor to use (overrides automatic selection)
    /// </summary>
    public static readonly BindableProperty EditorTypeProperty = BindableProperty.Create(
        nameof(EditorType),
        typeof(EditorType),
        typeof(CodeEditor),
        EditorType.Auto,
        propertyChanged: OnEditorConfigChanged);

    public EditorType EditorType
    {
        get => (EditorType)GetValue(EditorTypeProperty);
        set => SetValue(EditorTypeProperty, value);
    }

    /// <summary>
    /// The code content of the editor
    /// </summary>
    public static readonly BindableProperty CodeProperty = BindableProperty.Create(
        nameof(Code),
        typeof(string),
        typeof(CodeEditor),
        string.Empty,
        BindingMode.TwoWay,
        propertyChanged: OnCodeChanged);

    public string Code
    {
        get => (string)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    // Code input and output properties
    public string RawInput { get; set; } = "";
    public List<string> Input { get => RawInput.Split('\n').ToList(); }
    private string output = "";
    public string Output
    {
        get { return output; }
        set
        {
            output = value;
            OnPropertyChanged();
        }
    }
    private bool isRunning = false;
    public bool IsRunning
    {
        get { return isRunning; }
        set
        {
            isReadyToRun = !value;
            isRunning = value;
            OnPropertyChanged();
        }
    }
    private bool isReadyToRun = true;
    public bool IsReadyToRun
    {
        get { return isReadyToRun; }
        set
        {
            isReadyToRun = value;
            OnPropertyChanged();
        }
    }

    // Commands
    public ICommand RunCommand { get => new Command(RunCode); }

    #endregion

    public CodeEditor()
    {
        InitializeComponent();
        SetupWebView();
    }

    public async void RunCode()
    {
        IsRunning = true;
        var response = await executionRepository.RunCode(new()
        {
            Code = Code,
            ContainError = ShowError,
            ContainOutput = ShowOutput,
            Language = Language,
            Input = Input,
        });

        if (response.Success)
        {
            if (ShowOutput)
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
            if (ShowError)
            {
                Output = response.Error!;
            }
            else
            {
                Output = "Code execution failed";
            }
        }
        IsRunning = false;
    }

    private void SetupWebView()
    {
        webview.Navigated += OnWebViewNavigated;

#if WINDOWS
        webview.HandlerChanged += (s, e) =>
        {
            if (webview.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.WebView2 webView2)
            {
                webView2.WebMessageReceived += OnWebMessageReceived;
            }
        };
#endif

        LoadEditor();
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result == WebNavigationResult.Success)
        {
            // Give the editor a moment to initialize
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                _isEditorReady = true;

                // Apply any pending language change
                if (!string.IsNullOrEmpty(_pendingLanguage))
                {
                    await SetLanguageAsync(_pendingLanguage);
                    _pendingLanguage = null;
                }

                // Apply any pending code
                if (!string.IsNullOrEmpty(_pendingCode))
                {
                    await SetCodeAsync(_pendingCode);
                    _pendingCode = null;
                }
            });
        }
    }

#if WINDOWS
    private void OnWebMessageReceived(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
    {
		System.Diagnostics.Debug.WriteLine("WebView message received.");
        try
        {
            var message = args.WebMessageAsJson;
            var jsonDoc = JsonDocument.Parse(message);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "code")
            {
                if (root.TryGetProperty("content", out var contentElement))
                {
                    var code = contentElement.GetString() ?? string.Empty;
                    _getCodeTcs?.TrySetResult(code);
					Code = code;
					System.Diagnostics.Debug.WriteLine(code);
                }
            }
            else if (root.TryGetProperty("type", out var readyType) && readyType.GetString() == "ready")
            {
                _isEditorReady = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing WebView message: {ex.Message}");
        }
    }
#endif

    private static void OnLanguageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CodeEditor editor && newValue is string language)
        {
            _ = editor.SetLanguageInternalAsync(language);
        }
    }

    private static void OnCodeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CodeEditor editor && newValue is string code)
        {
            _ = editor.SetCodeInternalAsync(code);
        }
    }

    private static void OnEditorConfigChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CodeEditor editor)
        {
            editor.LoadEditor();
        }
    }

    private void LoadEditor()
    {
        var editorPath = GetEditorPath();
        webview.Source = editorPath;
        _isEditorReady = false;
    }

    private string GetEditorPath()
    {
        var selectedType = EditorType;

        if (selectedType == EditorType.Auto)
        {
            selectedType = DetermineEditorType();
        }

        return selectedType switch
        {
            EditorType.AllHelpers => "wwwroot/monaco/AllHelpersEditor/editor.html",
            EditorType.IntellisenseOnly => "wwwroot/monaco/IntellisenseOnlyEditor/editor.html",
            EditorType.NoHelpers => "wwwroot/monaco/NoHelpersEditor/editor.html",
            EditorType.ReadOnly => "wwwroot/monaco/ReadOnlyEditor/editor.html",
            EditorType.SignatureOnly => "wwwroot/monaco/SignatureOnlyEditor/editor.html",
            _ => "wwwroot/monaco/AllHelpersEditor/editor.html"
        };
    }

    private EditorType DetermineEditorType()
    {
        // If read-only is explicitly set, use ReadOnly editor
        if (IsReadOnly)
        {
            return EditorType.ReadOnly;
        }

        // If execution is not allowed, use a limited editor
        if (!AllowExecution)
        {
            return EditorType.NoHelpers;
        }

        // If output/error is hidden, use intellisense only
        if (!ShowOutput || !ShowError)
        {
            return EditorType.IntellisenseOnly;
        }

        // Default to full-featured editor
        return EditorType.AllHelpers;
    }

    private async Task SetCodeInternalAsync(string code)
    {
        if (!_isEditorReady)
        {
            _pendingCode = code;
            return;
        }

        if (await GetCodeAsync() != code)
            await SetCodeAsync(code);
    }

    private async Task SetLanguageInternalAsync(string language)
    {
        if (!_isEditorReady)
        {
            _pendingLanguage = language;
            return;
        }

        await SetLanguageAsync(language);
    }

    /// <summary>
    /// Sets the code in the editor
    /// </summary>
    public async Task SetCodeAsync(string code)
    {
        if (!_isEditorReady)
        {
            _pendingCode = code;
            return;
        }

        var escapedCode = JsonSerializer.Serialize(code);
        var script = $"window.postMessage({{ command: 'setText', text: {escapedCode} }}, '*');";
        await ExecuteJavaScriptAsync(script);
    }

    /// <summary>
    /// Gets the current code from the editor
    /// </summary>
    public async Task<string> GetCodeAsync()
    {
        if (!_isEditorReady)
        {
            return _pendingCode ?? string.Empty;
        }

        _getCodeTcs = new TaskCompletionSource<string>();

        var script = "window.postMessage({ command: 'getText' }, '*');";
        await ExecuteJavaScriptAsync(script);

        // Wait for the response with a timeout
        var timeoutTask = Task.Delay(5000);
        var completedTask = await Task.WhenAny(_getCodeTcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException("Failed to get code from editor within timeout period.");
        }

        return await _getCodeTcs.Task;
    }

    /// <summary>
    /// Sets the programming language of the editor
    /// </summary>
    public async Task SetLanguageAsync(string language)
    {
        if (!_isEditorReady)
        {
            _pendingLanguage = language;
            return;
        }

        var escapedLanguage = JsonSerializer.Serialize(language);
        var script = $"window.postMessage({{ command: 'setLanguage', language: {escapedLanguage} }}, '*');";
        await ExecuteJavaScriptAsync(script);
    }

    /// <summary>
    /// Configures the editor using QuestionConfiguration properties
    /// </summary>
    public void Configure(string language, bool allowExecution, bool showOutput, bool showError)
    {
        Language = language;
        AllowExecution = allowExecution;
        ShowOutput = showOutput;
        ShowError = showError;
    }

    private async Task ExecuteJavaScriptAsync(string script)
    {
#if WINDOWS
        if (webview.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.WebView2 webView2)
        {
            await webView2.ExecuteScriptAsync(script);
        }
#else
        await webview.EvaluateJavaScriptAsync(script);
#endif
    }
}

/// <summary>
/// Defines the type of Monaco editor to use
/// </summary>
public enum EditorType
{
    /// <summary>
    /// Automatically determine based on configuration
    /// </summary>
    Auto,

    /// <summary>
    /// Full-featured editor with all helpers (intellisense, parameter hints, etc.)
    /// </summary>
    AllHelpers,

    /// <summary>
    /// Editor with intellisense but no parameter hints
    /// </summary>
    IntellisenseOnly,

    /// <summary>
    /// Read-only editor with no helpers
    /// </summary>
    NoHelpers,

    /// <summary>
    /// Simple read-only editor
    /// </summary>
    ReadOnly,

    /// <summary>
    /// Read-only with signature help only
    /// </summary>
    SignatureOnly
}