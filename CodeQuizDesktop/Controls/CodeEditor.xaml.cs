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
    private bool _isSettingCodeFromWebView = false;
    private string? _initialCode;
    private bool _isInitialCodeSet = false;
    
    // Debounce timer for auto-save functionality
    private CancellationTokenSource? _debounceCts;
    private readonly TimeSpan _debounceDelay = TimeSpan.FromSeconds(2);
    
    /// <summary>
    /// Event fired when the user stops typing for the debounce period (2 seconds).
    /// Used to trigger auto-save functionality in parent components.
    /// </summary>
    public event EventHandler? CodeChangedDebounced;

    #region Bindable Properties

    /// <summary>
    /// The programming language for the editor
    /// </summary>
    public static readonly BindableProperty LanguageProperty = BindableProperty.Create(
        nameof(Language),
        typeof(string),
        typeof(CodeEditor),
        "python",
        propertyChanged: OnLanguageChanged);

    public string Language
    {
        get => ((string)GetValue(LanguageProperty)).ToLower();
        set => SetValue(LanguageProperty, value.ToLower());
    }

    /// <summary>
    /// Whether code execution is allowed
    /// </summary>
    public static readonly BindableProperty AllowExecutionProperty = BindableProperty.Create(
        nameof(AllowExecution),
        typeof(bool),
        typeof(CodeEditor),
        true);

    public bool AllowExecution
    {
        get => (bool)GetValue(AllowExecutionProperty);
        set => SetValue(AllowExecutionProperty, value);
    }

    /// <summary>
    /// Whether to show output
    /// </summary>
    public static readonly BindableProperty ShowOutputProperty = BindableProperty.Create(
        nameof(ShowOutput),
        typeof(bool),
        typeof(CodeEditor),
        true);

    public bool ShowOutput
    {
        get => (bool)GetValue(ShowOutputProperty);
        set => SetValue(ShowOutputProperty, value);
    }

    /// <summary>
    /// Whether to show errors
    /// </summary>
    public static readonly BindableProperty ShowErrorProperty = BindableProperty.Create(
        nameof(ShowError),
        typeof(bool),
        typeof(CodeEditor),
        true);

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
        false);

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// The type of editor to use
    /// </summary>
    public static readonly BindableProperty EditorTypeProperty = BindableProperty.Create(
        nameof(EditorType),
        typeof(EditorType),
        typeof(CodeEditor),
        EditorType.AllHelpers,
        propertyChanged: OnEditorTypeChanged);

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
    
    /// <summary>
    /// Whether auto-save on typing pause is enabled
    /// </summary>
    public static readonly BindableProperty EnableAutoSaveProperty = BindableProperty.Create(
        nameof(EnableAutoSave),
        typeof(bool),
        typeof(CodeEditor),
        false);

    public bool EnableAutoSave
    {
        get => (bool)GetValue(EnableAutoSaveProperty);
        set => SetValue(EnableAutoSaveProperty, value);
    }

    // Code input and output properties
    public string RawInput { get; set; } = "";
    public List<string> Input { get => RawInput.Split(['\n', ' ', '\r']).ToList(); }
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
            OnPropertyChanged(nameof(IsReadyToRun));
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
    public ICommand ResetCommand { get => new Command(ResetCode); }

    #endregion

    public CodeEditor()
    {
        InitializeComponent();
        SetupWebView();
    }

    public async void RunCode()
    {
        if (IsRunning) return;

        try
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
        }
        catch (Exception ex)
        {
            Output = $"Error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }

    public void ResetCode()
    {
        if (_initialCode != null)
        {
            Code = _initialCode;
        }
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
        if (e.Result != WebNavigationResult.Success)
        {
            // Handle load error (optional)
            System.Diagnostics.Debug.WriteLine($"WebView failed to load: {e.Result}");
        }
        // Do NOT set _isEditorReady = true here. 
        // Wait for the "ready" message from JS.
    }

#if WINDOWS
private async void OnWebMessageReceived(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
{
    try
    {
        var message = args.WebMessageAsJson;
        var jsonDoc = JsonDocument.Parse(message);
        var root = jsonDoc.RootElement;

        // 1. Handle "code" updates (User typing)
        if (root.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "code")
        {
            if (root.TryGetProperty("content", out var contentElement))
            {
                var code = contentElement.GetString() ?? string.Empty;
                
                // Complete the GetCodeAsync task if one is waiting
                _getCodeTcs?.TrySetResult(code);
                
                // Update properties without triggering the loop
                _isSettingCodeFromWebView = true;
                Code = code;
                _isSettingCodeFromWebView = false;
                
                // Trigger debounced auto-save if enabled
                if (EnableAutoSave)
                {
                    TriggerDebouncedAutoSave();
                }
            }
        }
        // 2. Handle "ready" signal (Monaco loaded)
        else if (root.TryGetProperty("type", out var readyType) && readyType.GetString() == "ready")
        {
            _isEditorReady = true;
            System.Diagnostics.Debug.WriteLine("Monaco Editor is Ready.");

            // Process Pending Language
            if (!string.IsNullOrEmpty(_pendingLanguage))
            {
                await SetLanguageAsync(_pendingLanguage);
                _pendingLanguage = null;
            }

            // Process Pending Code
            if (!string.IsNullOrEmpty(_pendingCode))
            {
                await SetCodeAsync(_pendingCode);
                _pendingCode = null;
            }
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error parsing WebView message: {ex.Message}");
    }
}
#endif

    /// <summary>
    /// Triggers the debounced auto-save mechanism. Cancels any pending debounce
    /// and starts a new timer. When the timer expires (user stopped typing),
    /// the CodeChangedDebounced event is fired.
    /// </summary>
    private void TriggerDebouncedAutoSave()
    {
        // Cancel any existing debounce timer
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;
        
        // Start a new debounce timer
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_debounceDelay, token);
                
                // If we weren't cancelled, fire the event on the main thread
                if (!token.IsCancellationRequested)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        CodeChangedDebounced?.Invoke(this, EventArgs.Empty);
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Debounce was cancelled by new typing, this is expected
            }
        }, token);
    }
    
    /// <summary>
    /// Cancels any pending debounce timer. Call this when disposing or
    /// when you want to prevent the auto-save from firing.
    /// </summary>
    public void CancelPendingAutoSave()
    {
        _debounceCts?.Cancel();
        _debounceCts = null;
    }

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
            if (!editor._isInitialCodeSet)
            {
                editor._initialCode = code;
                editor._isInitialCodeSet = true;
            }

            // Skip if the change came from the WebView itself
            if (editor._isSettingCodeFromWebView)
                return;
                
            _ = editor.SetCodeAsync(code);
        }
    }

    private static void OnEditorTypeChanged(BindableObject bindable, object oldValue, object newValue)
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
        return EditorType switch
        {
            EditorType.AllHelpers => "wwwroot/monaco/AllHelpersEditor/editor.html",
            EditorType.IntellisenseOnly => "wwwroot/monaco/IntellisenseOnlyEditor/editor.html",
            EditorType.NoHelpers => "wwwroot/monaco/NoHelpersEditor/editor.html",
            EditorType.ReadOnly => "wwwroot/monaco/ReadOnlyEditor/editor.html",
            EditorType.SignatureOnly => "wwwroot/monaco/SignatureOnlyEditor/editor.html",
            _ => "wwwroot/monaco/AllHelpersEditor/editor.html"
        };
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
    //public async Task SetLanguageAsync(string language)
    //{
    //    if (!_isEditorReady)
    //    {
    //        _pendingLanguage = language;
    //        return;
    //    }

    //    var escapedLanguage = JsonSerializer.Serialize(language);
    //    var script = $"window.postMessage({{ command: 'setLanguage', language: {escapedLanguage} }}, '*');";
    //    await ExecuteJavaScriptAsync(script);
    //}

    public async Task SetLanguageAsync(string language)
    {
        // 1. Normalize the language ID for Monaco
        string monacoId = MapToMonacoId(language);

        if (!_isEditorReady)
        {
            _pendingLanguage = monacoId;
            return;
        }

        // 2. Serialize to ensure safety
        var escapedId = JsonSerializer.Serialize(monacoId);

        // 3. Call the JS function directly (No event listener latency)
        var script = $"if (window.setLanguage) {{ window.setLanguage({escapedId}); }}";

        await ExecuteJavaScriptAsync(script);
    }

    // Helper to fix common ID mismatches
    private string MapToMonacoId(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "plaintext";

        var lower = input.ToLower().Trim();

        return lower switch
        {
            "c#" => "csharp",
            "cs" => "csharp",
            "c++" => "cpp",
            "js" => "javascript",
            "ts" => "typescript",
            "py" => "python",
            _ => lower // Fallback to whatever was passed (e.g., "html", "css", "java")
        };
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
    /// Full-featured editor with all helpers (intellisense, parameter hints, etc.)
    /// </summary>
    AllHelpers,

    /// <summary>
    /// Editor with intellisense but no parameter hints
    /// </summary>
    IntellisenseOnly,

    /// <summary>
    /// Editor with no helpers
    /// </summary>
    NoHelpers,

    /// <summary>
    /// Simple read-only editor
    /// </summary>
    ReadOnly,

    /// <summary>
    /// Editor with signature help only
    /// </summary>
    SignatureOnly
}