using CodeQuizDesktop.Views;

namespace CodeQuizDesktop
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ExaminerViewQuiz), typeof(ExaminerViewQuiz));
            Routing.RegisterRoute(nameof(GradeAttempt), typeof(GradeAttempt));
            Routing.RegisterRoute(nameof(ExamineeReviewQuiz), typeof(ExamineeReviewQuiz));
            Routing.RegisterRoute(nameof(EditQuiz), typeof(EditQuiz));
        }
    }
}
