using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using ClosedXML.Excel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CodeQuizDesktop.Viewmodels
{
    public class ExaminerViewQuizVM : BaseViewModel, IQueryAttributable
    {
        private readonly IQuizzesRepository _quizzesRepository;
        private readonly IAttemptsRepository _attemptsRepository;
        private readonly INavigationService _navigationService;
        private Action<ExaminerAttempt>? _attemptCreatedHandler;
        private Action<ExaminerAttempt>? _attemptUpdatedHandler;

        private ExaminerQuiz? quiz;

        public ExaminerQuiz? Quiz
        {
            get { return quiz; }
            set
            {
                quiz = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ExaminerAttempt> attempts = [];

        public ObservableCollection<ExaminerAttempt> Attempts
        {
            get { return attempts; }
            set
            {
                if (attempts != null)
                {
                    attempts.CollectionChanged -= OnAttemptsCollectionChanged;
                }
                attempts = value;
                if (attempts != null)
                {
                    attempts.CollectionChanged += OnAttemptsCollectionChanged;
                }
                OnPropertyChanged();
                UpdateStatistics();
            }
        }

        private void OnAttemptsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateStatistics();
        }

        #region Statistics Properties

        private QuizStatistics? _statistics;
        public QuizStatistics? Statistics
        {
            get => _statistics;
            set
            {
                _statistics = value;
                OnPropertyChanged();
                UpdateChartSeries();
            }
        }

        private ISeries[] _gradeCurveSeries = [];
        public ISeries[] GradeCurveSeries
        {
            get => _gradeCurveSeries;
            set
            {
                _gradeCurveSeries = value;
                OnPropertyChanged();
            }
        }

        // Default axis that satisfies LiveCharts2 requirement
        private static Axis[] CreateDefaultXAxes() =>
        [
            new Axis
            {
                Labels = ["0-10%", "10-20%", "20-30%", "30-40%", "40-50%", "50-60%", "60-70%", "70-80%", "80-90%", "90-100%"],
                LabelsRotation = 0,
                LabelsPaint = new SolidColorPaint(new SKColor(138, 138, 138)),
                TextSize = 11,
                SeparatorsPaint = new SolidColorPaint(new SKColor(42, 42, 42)) { StrokeThickness = 1 }
            }
        ];

        private static Axis[] CreateDefaultYAxes() =>
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(new SKColor(138, 138, 138)),
                TextSize = 11,
                SeparatorsPaint = new SolidColorPaint(new SKColor(42, 42, 42)) { StrokeThickness = 1 },
                MinLimit = 0
            }
        ];

        private Axis[] _xAxes = CreateDefaultXAxes();
        public Axis[] XAxes
        {
            get => _xAxes;
            set
            {
                // LiveCharts2 requires at least one axis - never set to empty
                _xAxes = value.Length > 0 ? value : CreateDefaultXAxes();
                OnPropertyChanged();
            }
        }

        private Axis[] _yAxes = CreateDefaultYAxes();
        public Axis[] YAxes
        {
            get => _yAxes;
            set
            {
                // LiveCharts2 requires at least one axis - never set to empty
                _yAxes = value.Length > 0 ? value : CreateDefaultYAxes();
                OnPropertyChanged();
            }
        }

        private bool _isStatisticsPanelVisible = true;
        public bool IsStatisticsPanelVisible
        {
            get => _isStatisticsPanelVisible;
            set
            {
                _isStatisticsPanelVisible = value;
                OnPropertyChanged();
            }
        }
        
        private bool _isChartVisible = false;
        /// <summary>
        /// Controls chart visibility - only show when we have valid data
        /// </summary>
        public bool IsChartVisible
        {
            get => _isChartVisible;
            set
            {
                _isChartVisible = value;
                OnPropertyChanged();
            }
        }

        private void UpdateStatistics()
        {
            if (Quiz == null || Attempts == null)
            {
                Statistics = null;
                return;
            }

            try
            {
                var totalPoints = Quiz.Questions?.Sum(q => q.Points) ?? 100f;
                Statistics = QuizStatistics.Calculate(Attempts, totalPoints);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating statistics: {ex.Message}");
                Statistics = null;
            }
        }

        private void UpdateChartSeries()
        {
            if (Statistics == null || Statistics.GradeDistribution.Count == 0)
            {
                GradeCurveSeries = [];
                IsChartVisible = false;
                return;
            }

            try
            {
                var actualCounts = Statistics.GradeDistribution.Select(b => b.Percentage).ToArray();
                var normalCounts = Statistics.NormalDistributionValues.ToArray();
                var labels = Statistics.GradeDistribution.Select(b => b.Label).ToArray();

                // Primary color from theme: #591C21
                var primaryColor = new SKColor(89, 28, 33);
                // Success color: #1F7A4D  
                var successColor = new SKColor(31, 122, 77);
                // Text colors
                var mutedColor = new SKColor(138, 138, 138);

                GradeCurveSeries =
                [
                    new ColumnSeries<double>
                    {
                        Values = actualCounts,
                        Name = "Actual Distribution (%)",
                        Fill = new SolidColorPaint(primaryColor),
                        MaxBarWidth = 30,
                        Padding = 2
                    },
                    new LineSeries<double>
                    {
                        Values = normalCounts,
                        Name = "Normal Curve",
                        Stroke = new SolidColorPaint(successColor) { StrokeThickness = 3 },
                        Fill = null,
                        GeometrySize = 0,
                        GeometryStroke = new SolidColorPaint(successColor) { StrokeThickness = 2 },
                        GeometryFill = new SolidColorPaint(new SKColor(31, 31, 31)),
                        LineSmoothness = 0.5
                    }
                ];

                XAxes =
                [
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 0,
                        LabelsPaint = new SolidColorPaint(mutedColor),
                        TextSize = 11,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(42, 42, 42)) { StrokeThickness = 1 }
                    }
                ];

                YAxes =
                [
                    new Axis
                    {
                        Name = "% of Students",
                        NamePaint = new SolidColorPaint(mutedColor),
                        NameTextSize = 12,
                        LabelsPaint = new SolidColorPaint(mutedColor),
                        TextSize = 11,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(42, 42, 42)) { StrokeThickness = 1 },
                        MinLimit = 0
                    }
                ];
                
                IsChartVisible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating chart series: {ex.Message}");
                GradeCurveSeries = [];
                IsChartVisible = false;
            }
        }

        #endregion

        public ICommand ReturnCommand { get => new Command(async () => await ReturnToPreviousPage()); }

        public async Task ReturnToPreviousPage()
        {
            try
            {
                // Hide chart first to prevent any rendering during cleanup
                IsChartVisible = false;
                
                // Leave the quiz group when navigating away
                if (Quiz != null)
                {
                    try
                    {
                        await _attemptsRepository.LeaveQuizGroupAsync(Quiz.Id);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error leaving quiz group: {ex.Message}");
                    }
                }
                UnsubscribeFromUpdates();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
            
            await _navigationService.GoToAsync("..");
        }

        public ICommand GoToGradeAttemptPageCommand { get => new Command<ExaminerAttempt>(async (a) => await OnGoToGradeAttemptPage(a)); }

        public async Task OnGoToGradeAttemptPage(ExaminerAttempt examinerAttempt)
        {
            await _navigationService.GoToAsync(nameof(GradeAttempt), new Dictionary<string, object>
            {
                { "attempt", examinerAttempt! },
                { "quiz", Quiz! }
            });
        }

        public ICommand SaveGradesReportCommand { get => new Command(async () => await OnSaveGradesReport()); }

        public async Task OnSaveGradesReport()
        {
            if (Attempts == null || Attempts.Count == 0)
                return;
                
            await ExportToExcel(Attempts.ToList());
        }

        public ICommand ToggleStatisticsPanelCommand { get => new Command(() => IsStatisticsPanelVisible = !IsStatisticsPanelVisible); }

        public async Task ExportToExcel(List<ExaminerAttempt> examinerAttempts)
        {
            if (examinerAttempts.Count == 0)
                return;
                
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Grades Report");

            // Header row
            ws.Cell(1, 1).Value = "Username";
            ws.Cell(1, 2).Value = "Full Name";
            ws.Cell(1, 3).Value = "Submission Date";

            int maxQs = examinerAttempts.Max(a => a.Solutions.Count);
            for (int i = 0; i < maxQs; i++)
                ws.Cell(1, i + 4).Value = $"Q{i + 1}";

            ws.Cell(1, maxQs + 4).Value = "Total Grade";

            // Data rows
            for (int i = 0; i < examinerAttempts.Count; i++)
            {
                var a = examinerAttempts[i];
                int row = i + 2;

                ws.Cell(row, 1).Value = a.Examinee.UserName;
                ws.Cell(row, 2).Value = a.Examinee.FullName;


                ws.Cell(row, 3).Value = a.EndTime;
                ws.Cell(row, 3).Style.DateFormat.Format = "yyyy-mm-dd hh:mm AM/PM";

                for (int j = 0; j < a.Solutions.Count; j++)
                    ws.Cell(row, j + 4).Value = a.Solutions[j].ReceivedGrade;

                ws.Cell(row, maxQs + 4).Value = a.Grade;
            }

            // Styling
            var headerRange = ws.Range(1, 1, 1, maxQs + 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#591c21");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Add borders to the data
            var dataRange = ws.Range(1, 1, examinerAttempts.Count + 1, maxQs + 4);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Auto-fit columns
            ws.Columns().AdjustToContents();

            // Save and Export
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            await FileSaver.Default.SaveAsync("GradesReport.xlsx", stream, default);
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("quiz") && query["quiz"] is ExaminerQuiz receivedQuiz)
            {
                Quiz = receivedQuiz;
                await LoadAttemptsAsync();
                await SubscribeToRealTimeUpdatesAsync();
            }
        }

        public async Task LoadAttemptsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var response = await _quizzesRepository.GetQuizAttempts(Quiz!.Id);
                Attempts = response.ToObservableCollection();
            }, "Loading attempts...");
        }
        
        /// <summary>
        /// Subscribe to real-time attempt updates for the current quiz
        /// </summary>
        private async Task SubscribeToRealTimeUpdatesAsync()
        {
            if (Quiz == null) return;
            
            try
            {
                // Join the quiz group to receive real-time updates
                await _attemptsRepository.JoinQuizGroupAsync(Quiz.Id);
                
                // Create handlers that filter by quiz ID and marshal to UI thread
                _attemptCreatedHandler = (attempt) =>
                {
                    // Only process if this attempt belongs to our quiz
                    if (attempt.QuizId != Quiz.Id) return;
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            // Check if attempt already exists
                            if (Attempts.Any(a => a.Id == attempt.Id)) return;
                            
                            Attempts.Add(attempt);
                            System.Diagnostics.Debug.WriteLine($"Real-time: New attempt added for quiz {Quiz.Id}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error adding attempt: {ex.Message}");
                        }
                    });
                };
                
                _attemptUpdatedHandler = (attempt) =>
                {
                    // Only process if this attempt belongs to our quiz
                    if (attempt.QuizId != Quiz.Id) return;
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            var existingAttempt = Attempts.FirstOrDefault(a => a.Id == attempt.Id);
                            if (existingAttempt != null)
                            {
                                var idx = Attempts.IndexOf(existingAttempt);
                                Attempts.Remove(existingAttempt);
                                Attempts.Insert(idx, attempt);
                                System.Diagnostics.Debug.WriteLine($"Real-time: Attempt {attempt.Id} updated for quiz {Quiz.Id}");
                            }
                            else
                            {
                                // Attempt not found, add it (might have been created while loading)
                                Attempts.Add(attempt);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error updating attempt: {ex.Message}");
                        }
                    });
                };
                
                _attemptsRepository.SubscribeCreate(_attemptCreatedHandler);
                _attemptsRepository.SubscribeUpdate(_attemptUpdatedHandler);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error subscribing to real-time updates: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Unsubscribe from real-time updates when leaving the view
        /// </summary>
        private void UnsubscribeFromUpdates()
        {
            try
            {
                if (_attemptCreatedHandler != null)
                {
                    _attemptsRepository.UnsubscribeCreate(_attemptCreatedHandler);
                    _attemptCreatedHandler = null;
                }
                
                if (_attemptUpdatedHandler != null)
                {
                    _attemptsRepository.UnsubscribeUpdate(_attemptUpdatedHandler);
                    _attemptUpdatedHandler = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unsubscribing from updates: {ex.Message}");
            }
        }

        public ExaminerViewQuizVM(IQuizzesRepository quizzesRepository, IAttemptsRepository attemptsRepository, INavigationService navigationService)
        {
            _quizzesRepository = quizzesRepository;
            _attemptsRepository = attemptsRepository;
            _navigationService = navigationService;
        }
    }
}
