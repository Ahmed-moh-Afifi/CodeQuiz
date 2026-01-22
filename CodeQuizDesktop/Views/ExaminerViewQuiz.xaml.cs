using CodeQuizDesktop.Viewmodels;
using LiveChartsCore.SkiaSharpView.Maui;

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

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		// Restore the chart when navigating back to this page
		RestoreChart();
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

	private void RestoreChart()
	{
		try
		{
			// Recreate the chart if it was removed
			if (chartContainer != null && chartContainer.Content == null)
			{
				var newChart = new CartesianChart
				{
					HeightRequest = 200,
					BackgroundColor = Colors.Transparent
				};

				// Set bindings
				newChart.SetBinding(CartesianChart.SeriesProperty, new Binding("GradeCurveSeries"));
				newChart.SetBinding(CartesianChart.XAxesProperty, new Binding("XAxes"));
				newChart.SetBinding(CartesianChart.YAxesProperty, new Binding("YAxes"));

				chartContainer.Content = newChart;
			}

			// Trigger chart update
			_viewModel.RestoreChart();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Chart restore error: {ex.Message}");
		}
	}

	private void CleanupChart()
	{
		try
		{
			// Hide chart via ViewModel
			_viewModel.IsChartVisible = false;

			// Physically remove the chart from the visual tree to prevent handler disconnection issues
			if (chartContainer != null && chartContainer.Content is CartesianChart chart)
			{
				// Clear bindings first
				chart.Series = null;
				chart.XAxes = null;
				chart.YAxes = null;

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