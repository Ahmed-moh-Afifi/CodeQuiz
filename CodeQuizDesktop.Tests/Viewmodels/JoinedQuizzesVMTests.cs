using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using CodeQuizDesktop.Views;
using FluentAssertions;
using Moq;
using System.Collections.ObjectModel;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class JoinedQuizzesVMTests
    {
        private readonly Mock<IAttemptsRepository> _attemptsRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly JoinedQuizzesVM _viewModel;

        public JoinedQuizzesVMTests()
        {
            _attemptsRepoMock = new Mock<IAttemptsRepository>();
            _navServiceMock = new Mock<INavigationService>();
            _viewModel = new JoinedQuizzesVM(_attemptsRepoMock.Object, _navServiceMock.Object);
        }

        private ExamineeAttempt CreateMockAttempt()
        {
            return new ExamineeAttempt
            {
                Id = 1,
                StartTime = DateTime.Now,
                QuizId = 1,
                ExamineeId = "2",
                MaxEndTime = DateTime.Now.AddHours(1),
                Solutions = new List<Solution>(),
                Quiz = new ExamineeQuiz
                {
                    Id = 1,
                    Code = "TESTCODE",
                    Title = "Test Quiz",
                    ExaminerId = "1",
                    Examiner = new User { Id = "1", FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test", JoinDate = DateTime.Now },
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(1),
                    Duration = TimeSpan.FromHours(1),
                    GlobalQuestionConfiguration = new QuestionConfiguration
                    {
                        Language = "csharp",
                        AllowExecution = true,
                        ShowOutput = true,
                        ShowError = true
                    },
                    AllowMultipleAttempts = true,
                    Questions = new List<Question>(),
                    QustionsCount = 0,
                    TotalPoints = 100
                }
            };
        }

        [Fact]
        public async Task JoinQuizAsync_ShouldNavigateToJoinQuiz_WhenQuizCodeIsValid()
        {
            // Arrange
            _viewModel.QuizCode = "TESTCODE";
            var attempt = CreateMockAttempt();
            _attemptsRepoMock.Setup(x => x.BeginAttempt(It.IsAny<BeginAttemptRequest>())).ReturnsAsync(attempt);

            // Act
            await _viewModel.JoinQuizAsync();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync(nameof(JoinQuiz), It.Is<Dictionary<string, object>>(d => d.ContainsKey("attempt") && d["attempt"] == attempt)), Times.Once);
        }

        [Fact]
        public async Task JoinQuizAsync_ShouldNotNavigate_WhenQuizCodeIsEmpty()
        {
            // Arrange
            _viewModel.QuizCode = "";

            // Act
            await _viewModel.JoinQuizAsync();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

        [Fact]
        public async Task OnContinueAttemptAsync_ShouldNavigateToJoinQuiz()
        {
            // Arrange
            var attempt = CreateMockAttempt();
            _attemptsRepoMock.Setup(x => x.BeginAttempt(It.IsAny<BeginAttemptRequest>())).ReturnsAsync(attempt);

            // Act
            await _viewModel.OnContinueAttemptAsync(attempt);

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync(nameof(JoinQuiz), It.Is<Dictionary<string, object>>(d => d.ContainsKey("attempt") && d["attempt"] == attempt)), Times.Once);
        }

        [Fact]
        public async Task OnReviewAttempt_ShouldNavigateToExamineeReviewQuiz()
        {
            // Arrange
            var attempt = CreateMockAttempt();

            // Act
            await _viewModel.OnReviewAttempt(attempt);

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync(nameof(ExamineeReviewQuiz), It.Is<Dictionary<string, object>>(d => d.ContainsKey("attempt") && d["attempt"] == attempt)), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_ShouldLoadAttempts()
        {
            // Arrange
            var attempts = new List<ExamineeAttempt> { CreateMockAttempt() };
            _attemptsRepoMock.Setup(x => x.GetUserAttempts()).ReturnsAsync(attempts);

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            _viewModel.AllExamineeAttempts.Should().HaveCount(1);
            _viewModel.AllExamineeAttempts.First().Id.Should().Be(1);
        }
    }
}
