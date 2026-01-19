namespace CodeQuizDesktop.Views;

public partial class SuccessDialog : ContentView
{
    private TaskCompletionSource<bool>? _taskCompletionSource;

    public SuccessDialog()
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

    public string OkButtonText
    {
        get => okButton.Text;
        set => okButton.Text = value;
    }

    public Task ShowAsync(string title, string message, string okText = "OK")
    {
        Title = title;
        Message = message;
        OkButtonText = okText;

        _taskCompletionSource = new TaskCompletionSource<bool>();
        return _taskCompletionSource.Task;
    }

    private void OnOkClicked(object? sender, EventArgs e)
    {
        CloseDialog();
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        CloseDialog();
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        // Optional: close on overlay tap
        CloseDialog();
    }

    private void CloseDialog()
    {
        _taskCompletionSource?.TrySetResult(true);
    }
}
