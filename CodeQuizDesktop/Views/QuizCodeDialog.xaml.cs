namespace CodeQuizDesktop.Views;

/// <summary>
/// Dialog for displaying a quiz code with options to copy and share.
/// </summary>
public partial class QuizCodeDialog : ContentView
{
    private TaskCompletionSource<QuizCodeDialogResult>? _taskCompletionSource;
    private string _code = string.Empty;

    public QuizCodeDialog()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => titleLabel.Text;
        set => titleLabel.Text = value;
    }

    public string Subtitle
    {
        get => subtitleLabel.Text;
        set => subtitleLabel.Text = value;
    }

    public string Code
    {
        get => _code;
        set
        {
            _code = value;
            codeLabel.Text = value;
        }
    }

    public string HintText
    {
        get => hintLabel.Text;
        set => hintLabel.Text = value;
    }

    public string CopyButtonText
    {
        get => copyButton.Text;
        set => copyButton.Text = value;
    }

    public string ShareButtonText
    {
        get => shareButton.Text;
        set => shareButton.Text = value;
    }

    public string CancelButtonText
    {
        get => cancelButton.Text;
        set => cancelButton.Text = value;
    }

    public bool ShowShareButton
    {
        get => shareButton.IsVisible;
        set => shareButton.IsVisible = value;
    }

    /// <summary>
    /// Shows the dialog with the specified quiz code.
    /// </summary>
    /// <param name="code">The quiz code to display.</param>
    /// <param name="title">Optional title for the dialog.</param>
    /// <param name="subtitle">Optional subtitle for the dialog.</param>
    /// <param name="showShareButton">Whether to show the share button.</param>
    /// <returns>The result of the dialog interaction.</returns>
    public Task<QuizCodeDialogResult> ShowAsync(
        string code,
        string? title = null,
        string? subtitle = null,
        bool showShareButton = true)
    {
        Code = code;

        if (title != null)
            Title = title;

        if (subtitle != null)
            Subtitle = subtitle;

        ShowShareButton = showShareButton;

        _taskCompletionSource = new TaskCompletionSource<QuizCodeDialogResult>();
        return _taskCompletionSource.Task;
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(_code);

            // Visual feedback - temporarily change button text
            var originalText = copyButton.Text;
            copyButton.Text = "Copied!";
            await Task.Delay(1500);
            copyButton.Text = originalText;
        }
        catch
        {
            // If clipboard fails, still close the dialog
        }

        CloseDialog(QuizCodeDialogResult.Copied);
    }

    private async void OnShareClicked(object? sender, EventArgs e)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = $"Join my quiz using code: {_code}",
                Title = "Share Quiz Code"
            });
        }
        catch
        {
            // If share fails, still close the dialog
        }

        CloseDialog(QuizCodeDialogResult.Shared);
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        CloseDialog(QuizCodeDialogResult.Cancelled);
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        CloseDialog(QuizCodeDialogResult.Cancelled);
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        // Optional: close on overlay tap
        CloseDialog(QuizCodeDialogResult.Cancelled);
    }

    private void CloseDialog(QuizCodeDialogResult result)
    {
        _taskCompletionSource?.TrySetResult(result);
    }
}

/// <summary>
/// Result of the quiz code dialog interaction.
/// </summary>
public enum QuizCodeDialogResult
{
    /// <summary>
    /// User cancelled or closed the dialog.
    /// </summary>
    Cancelled,

    /// <summary>
    /// User copied the code to clipboard.
    /// </summary>
    Copied,

    /// <summary>
    /// User shared the code.
    /// </summary>
    Shared
}
