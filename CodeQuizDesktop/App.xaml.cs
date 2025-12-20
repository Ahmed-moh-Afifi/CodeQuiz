using CodeQuizDesktop.Services;

namespace CodeQuizDesktop
{
    public partial class App : Application
    {
        private readonly GlobalExceptionHandler _exceptionHandler;

        public App(GlobalExceptionHandler exceptionHandler)
        {
            InitializeComponent();

            _exceptionHandler = exceptionHandler;
            _exceptionHandler.Initialize();

            MainPage = new AppShell();
        }

        protected override void CleanUp()
        {
            _exceptionHandler.Cleanup();
            base.CleanUp();
        }
    }
}
