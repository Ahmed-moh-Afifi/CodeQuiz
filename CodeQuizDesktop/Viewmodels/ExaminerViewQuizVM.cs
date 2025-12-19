using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ClosedXML.Excel;

namespace CodeQuizDesktop.Viewmodels
{
    public class ExaminerViewQuizVM : BaseViewModel, IQueryAttributable
    {
        private readonly IQuizzesRepository _quizzesRepository;

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
                attempts = value;
                OnPropertyChanged();
            }
        }

        public ICommand ReturnCommand { get => new Command(ReturnToPreviousPage); }

        private async void ReturnToPreviousPage()
        {
            await Shell.Current.GoToAsync("..");
        }

        public ICommand GoToGradeAttemptPageCommand { get => new Command<ExaminerAttempt>(OnGoToGradeAttemptPage); }
        
        private async void OnGoToGradeAttemptPage(ExaminerAttempt examinerAttempt)
        {
            await Shell.Current.GoToAsync(nameof(GradeAttempt), new Dictionary<string, object>
            {
                { "attempt", examinerAttempt! },
                { "quiz", Quiz! }
            });
        }

        public ICommand SaveGradesReportCommand { get => new Command(OnSaveGradesReport); }

        private async void OnSaveGradesReport()
        {
            await ExportToExcel(Attempts.ToList());
        }

        public async Task ExportToExcel(List<ExaminerAttempt> examinerAttempts)
        {
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
            }
        }

        private async Task LoadAttemptsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var response = await _quizzesRepository.GetQuizAttempts(Quiz!.Id);
                Attempts = response.ToObservableCollection();
            }, "Loading attempts...");
        }

        public ExaminerViewQuizVM(IQuizzesRepository quizzesRepository)
        {
            _quizzesRepository = quizzesRepository;
        }
    }
}
