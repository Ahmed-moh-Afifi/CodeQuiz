using CodeQuizDesktop.Viewmodels;

namespace CodeQuizDesktop.Views;

public partial class ExaminerViewQuiz : ContentPage
{
	private readonly ExaminerViewQuizVM _viewModel;

	public ExaminerViewQuiz(ExaminerViewQuizVM examinerViewQuizVM)
	{
		InitializeComponent();
		_viewModel = examinerViewQuizVM;
		BindingContext = examinerViewQuizVM;
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		// Remove chart from visual tree BEFORE base navigation to prevent LiveCharts2 disposal exception
		CleanupChart();
		base.OnNavigatedFrom(args);
	}

	protected override void OnDisappearing()
	{
		// Also cleanup on disappearing as a fallback
		CleanupChart();
		base.OnDisappearing();
	}

	private void CleanupChart()
	{
		try
		{
			// Hide chart via ViewModel
			_viewModel.IsChartVisible = false;
			
			// Physically remove the chart from the visual tree to prevent handler disconnection issues
			if (chartContainer != null && gradeChart != null)
			{
				// Clear bindings first
				gradeChart.Series = null;
				gradeChart.XAxes = null;
				gradeChart.YAxes = null;
				
				// Remove from container
				chartContainer.Content = null;
			}
		}
		catch (Exception ex)
		{
			// Suppress any cleanup errors - this is a known LiveCharts2 issue
			System.Diagnostics.Debug.WriteLine($"Chart cleanup (expected): {ex.Message}");
		}
	}
}