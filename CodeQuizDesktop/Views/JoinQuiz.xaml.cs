using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class JoinQuiz : ContentPage
{
	private readonly JoinQuizVM _viewModel;
	
	public JoinQuiz(JoinQuizVM joinQuizVM)
	{
		InitializeComponent();
		_viewModel = joinQuizVM;
		BindingContext = _viewModel;
		
		// Wire up the debounced auto-save event from the CodeEditor
		codeEditor.CodeChangedDebounced += _viewModel.OnAutoSaveTriggered;
	}
	
	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		
		// Clean up: cancel any pending auto-save and unsubscribe
		codeEditor.CancelPendingAutoSave();
		codeEditor.CodeChangedDebounced -= _viewModel.OnAutoSaveTriggered;
	}
}