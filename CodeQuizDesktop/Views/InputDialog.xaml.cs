namespace CodeQuizDesktop.Views;

public partial class InputDialog : ContentView
{
    private TaskCompletionSource<string?>? _taskCompletionSource;

    public InputDialog()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => titleLabel.Text;
        set => titleLabel.Text = value;
    }

    public string Message
    {
        get => messageLabel.Text;
        set => messageLabel.Text = value;
    }

    public string Placeholder
    {
        get => inputEntry.Placeholder;
        set => inputEntry.Placeholder = value;
    }

    public string InputText
    {
        get => inputEntry.Text ?? string.Empty;
        set => inputEntry.Text = value;
    }

    public Keyboard Keyboard
    {
        get => inputEntry.Keyboard;
        set => inputEntry.Keyboard = value;
    }

    public string SubmitButtonText
    {
        get => submitButton.Text;
        set => submitButton.Text = value;
    }

    public string CancelButtonText
    {
        get => cancelButton.Text;
        set => cancelButton.Text = value;
    }

    public string IconSource
    {
        set => headerIcon.Source = value;
    }

    public Task<string?> ShowAsync(
        string title,
        string message,
        string placeholder = "",
        string submitText = "Submit",
        string cancelText = "Cancel",
        Keyboard? keyboard = null,
        string initialValue = "",
        string iconSource = "question.png")
    {
        Title = title;
        Message = message;
        Placeholder = placeholder;
        SubmitButtonText = submitText;
        CancelButtonText = cancelText;
        InputText = initialValue;
        IconSource = iconSource;

        if (keyboard != null)
        {
            Keyboard = keyboard;
        }

        _taskCompletionSource = new TaskCompletionSource<string?>();

        // Focus the entry after a short delay to ensure UI is ready
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
        {
            inputEntry.Focus();
        });

        return _taskCompletionSource.Task;
    }

    private void OnSubmitClicked(object? sender, EventArgs e)
    {
        Submit();
    }

    private void OnInputCompleted(object? sender, EventArgs e)
    {
        Submit();
    }

    private void Submit()
    {
        var text = InputText?.Trim();
        if (!string.IsNullOrWhiteSpace(text))
        {
            _taskCompletionSource?.TrySetResult(text);
        }
        else
        {
            // Return null to indicate empty/cancelled
            _taskCompletionSource?.TrySetResult(null);
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        _taskCompletionSource?.TrySetResult(null);
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        // Treat overlay tap as cancel
        _taskCompletionSource?.TrySetResult(null);
    }
}
