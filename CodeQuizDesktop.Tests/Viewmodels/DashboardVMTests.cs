using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using FluentAssertions;
using Moq;
using System.Collections.ObjectModel;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class DashboardVMTests
    {
        private readonly Mock<IAttemptsRepository> _attemptsRepoMock;
        private readonly Mock<IQuizzesRepository> _quizzesRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly DashboardVM _viewModel;

        public DashboardVMTests()
        {
            _attemptsRepoMock = new Mock<IAttemptsRepository>();
            _quizzesRepoMock = new Mock<IQuizzesRepository>();
            _navServiceMock = new Mock<INavigationService>();

            // Setup default returns for Initialize
            _attemptsRepoMock.Setup(x => x.GetUserAttempts()).ReturnsAsync(new List<ExamineeAttempt>());
            _quizzesRepoMock.Setup(x => x.GetUserQuizzes()).ReturnsAsync(new List<ExaminerQuiz>());

            _viewModel = new DashboardVM(_attemptsRepoMock.Object, _quizzesRepoMock.Object, _navServiceMock.Object);
        }

        [Fact]
        public async Task ContinueAttemptAsync_ShouldCallRepositoryAndNavigate()
        {
            // Arrange
            var attempt = new ExamineeAttempt
            {
                Id = 1,
                StartTime = DateTime.Now,
                QuizId = 1,
                ExamineeId = "user1",
                Solutions = new List<Solution>(),
                MaxEndTime = DateTime.Now.AddHours(1),
                Quiz = new ExamineeQuiz
                {
                    Id = 1,
                    Code = "12345",
                    Title = "Test Quiz",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    Duration = TimeSpan.FromHours(1),
                    ExaminerId = "examiner1",
                    GlobalQuestionConfiguration = new QuestionConfiguration
                    {
                        Language = "csharp",
                        AllowExecution = true,
                        ShowOutput = true,
                        ShowError = true
                    },
                    AllowMultipleAttempts = false,
                    Questions = new List<Question>(),
                    TotalPoints = 100,
                    Examiner = new User { Id = "examiner1", FirstName = "Ex", LastName = "Am", Email = "ex@am.com", UserName = "exam", JoinDate = DateTime.Now },
                    QustionsCount = 0
                }
            };
            var response = new ExamineeAttempt
            {
                Id = 1,
                StartTime = DateTime.Now,
                QuizId = 1,
                ExamineeId = "user1",
                Solutions = new List<Solution>(),
                MaxEndTime = DateTime.Now.AddHours(1),
                Quiz = attempt.Quiz
            };

            _attemptsRepoMock.Setup(x => x.BeginAttempt(It.IsAny<BeginAttemptRequest>()))
                .ReturnsAsync(response);

            // Act
            await _viewModel.ContinueAttemptAsync(attempt);

            // Assert
            _attemptsRepoMock.Verify(x => x.BeginAttempt(It.Is<BeginAttemptRequest>(r => r.QuizCode == "12345")), Times.Once);
            _navServiceMock.Verify(x => x.GoToAsync("///JoinQuizPage", It.Is<Dictionary<string, object>>(d => d.ContainsKey("attempt") && d["attempt"] == response)), Times.Once);
        }

        [Fact]
        public async Task ViewResultsAsync_ShouldNavigateToReviewPage()
        {
            // Arrange
            var attempt = new ExamineeAttempt
            {
                Id = 1,
                StartTime = DateTime.Now,
                QuizId = 1,
                ExamineeId = "user1",
                Solutions = new List<Solution>(),
                MaxEndTime = DateTime.Now.AddHours(1),
                Quiz = new ExamineeQuiz
                {
                    Id = 1,
                    Code = "12345",
                    Title = "Test Quiz",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    Duration = TimeSpan.FromHours(1),
                    ExaminerId = "examiner1",
                    GlobalQuestionConfiguration = new QuestionConfiguration
                    {
                        Language = "csharp",
                        AllowExecution = true,
                        ShowOutput = true,
                        ShowError = true
                    },
                    AllowMultipleAttempts = false,
                    Questions = new List<Question>(),
                    TotalPoints = 100,
                    Examiner = new User { Id = "examiner1", FirstName = "Ex", LastName = "Am", Email = "ex@am.com", UserName = "exam", JoinDate = DateTime.Now },
                    QustionsCount = 0
                }
            };

            // Act
            await _viewModel.ViewResultsAsync(attempt);

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("ExamineeReviewQuiz", It.Is<Dictionary<string, object>>(d => d.ContainsKey("attempt") && d["attempt"] == attempt)), Times.Once);
        }

        [Fact]
        public async Task ViewCreatedQuizAsync_ShouldNavigateToExaminerViewPage()
        {
            // Arrange
            var quiz = new ExaminerQuiz
            {
                Id = 1,
                Title = "Test Quiz",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Duration = TimeSpan.FromHours(1),
                Code = "12345",
                ExaminerId = "examiner1",
                GlobalQuestionConfiguration = new QuestionConfiguration
                {
                    Language = "csharp",
                    AllowExecution = true,
                    ShowOutput = true,
                    ShowError = true
                },
                AllowMultipleAttempts = false,
                Questions = new List<Question>(),
                QustionsCount = 0,
                AttemptsCount = 0,
                SubmittedAttemptsCount = 0,
                AverageAttemptScore = 0,
                TotalPoints = 100
            };

            // Act
            await _viewModel.ViewCreatedQuizAsync(quiz);

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("ExaminerViewQuiz", It.Is<Dictionary<string, object>>(d => d.ContainsKey("quiz") && d["quiz"] == quiz)), Times.Once);
        }

        [Fact]
        public async Task DeleteCreatedQuizAsync_ShouldDeleteAndRemoveFromCollection_WhenConfirmed()
        {
            // Arrange
            var quiz = new ExaminerQuiz
            {
                Id = 1,
                Title = "Test Quiz",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Duration = TimeSpan.FromHours(1),
                Code = "12345",
                ExaminerId = "examiner1",
                GlobalQuestionConfiguration = new QuestionConfiguration
                {
                    Language = "csharp",
                    AllowExecution = true,
                    ShowOutput = true,
                    ShowError = true
                },
                AllowMultipleAttempts = false,
                Questions = new List<Question>(),
                QustionsCount = 0,
                AttemptsCount = 0,
                SubmittedAttemptsCount = 0,
                AverageAttemptScore = 0,
                TotalPoints = 100
            };
            _viewModel.CreatedQuizzes.Add(quiz);

            _navServiceMock.Setup(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), "Delete", "Cancel"))
                .ReturnsAsync(true);

            // Act
            await _viewModel.DeleteCreatedQuizAsync(quiz);

            // Assert
            _quizzesRepoMock.Verify(x => x.DeleteQuiz(1), Times.Once);
            _viewModel.CreatedQuizzes.Should().NotContain(quiz);
        }

        [Fact]
        public async Task DeleteCreatedQuizAsync_ShouldNotDelete_WhenCancelled()
        {
            // Arrange
            var quiz = new ExaminerQuiz
            {
                Id = 1,
                Title = "Test Quiz",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Duration = TimeSpan.FromHours(1),
                Code = "12345",
                ExaminerId = "examiner1",
                GlobalQuestionConfiguration = new QuestionConfiguration
                {
                    Language = "csharp",
                    AllowExecution = true,
                    ShowOutput = true,
                    ShowError = true
                },
                AllowMultipleAttempts = false,
                Questions = new List<Question>(),
                QustionsCount = 0,
                AttemptsCount = 0,
                SubmittedAttemptsCount = 0,
                AverageAttemptScore = 0,
                TotalPoints = 100
            };
            _viewModel.CreatedQuizzes.Add(quiz);

            _navServiceMock.Setup(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), "Delete", "Cancel"))
                .ReturnsAsync(false);

            // Act
            await _viewModel.DeleteCreatedQuizAsync(quiz);

            // Assert
            _quizzesRepoMock.Verify(x => x.DeleteQuiz(It.IsAny<int>()), Times.Never);
            _viewModel.CreatedQuizzes.Should().Contain(quiz);
        }
    }
}