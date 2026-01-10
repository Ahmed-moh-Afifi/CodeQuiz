namespace CodeQuizDesktop.Views;

public partial class ConfirmationDialog : ContentView
{
    private TaskCompletionSource<bool>? _taskCompletionSource;

    public ConfirmationDialog()
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

    public string ConfirmButtonText
    {
        get => confirmButton.Text;
        set => confirmButton.Text = value;
    }

    public string CancelButtonText
    {
        get => cancelButton.Text;
        set => cancelButton.Text = value;
    }

    /// <summary>
    /// Sets the dialog to use a destructive/danger style for the confirm button.
    /// </summary>
    public bool IsDestructive
    {
        set
        {
            if (value)
            {
                confirmButton.Style = (Style)Application.Current!.Resources["DangerButton"];
                iconContainer.BackgroundColor = (Color)Application.Current.Resources["ErrorTransparent"];
                // Update icon tint color for destructive actions
                headerIcon.Source = "warning.png";
            }
            else
            {
                confirmButton.Style = (Style)Application.Current!.Resources["PrimaryButton"];
                iconContainer.BackgroundColor = (Color)Application.Current.Resources["PrimaryTransparent"];
                headerIcon.Source = "question.png";
            }
        }
    }

    public Task<bool> ShowAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel", bool isDestructive = false)
    {
        Title = title;
        Message = message;
        ConfirmButtonText = confirmText;
        CancelButtonText = cancelText;
        IsDestructive = isDestructive;

        _taskCompletionSource = new TaskCompletionSource<bool>();
        return _taskCompletionSource.Task;
    }

    private void OnConfirmClicked(object? sender, EventArgs e)
    {
        _taskCompletionSource?.TrySetResult(true);
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        _taskCompletionSource?.TrySetResult(false);
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        // Treat overlay tap as cancel
        _taskCompletionSource?.TrySetResult(false);
    }
}
