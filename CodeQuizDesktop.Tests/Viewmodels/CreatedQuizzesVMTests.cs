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
    public class CreatedQuizzesVMTests
    {
        private readonly Mock<IQuizzesRepository> _quizzesRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly CreatedQuizzesVM _viewModel;

        public CreatedQuizzesVMTests()
        {
            _quizzesRepoMock = new Mock<IQuizzesRepository>();
            _navServiceMock = new Mock<INavigationService>();
            _viewModel = new CreatedQuizzesVM(_quizzesRepoMock.Object, _navServiceMock.Object);
        }

        private ExaminerQuiz CreateMockQuiz()
        {
            return new ExaminerQuiz
            {
                Id = 1,
                Code = "TESTCODE",
                Title = "Test Quiz",
                ExaminerId = "1",
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
                AttemptsCount = 0,
                SubmittedAttemptsCount = 0,
                AverageAttemptScore = 0,
                TotalPoints = 100
            };
        }

        [Fact]
        public async Task OnCreateQuizPage_ShouldNavigateToCreateQuiz()
        {
            // Act
            await _viewModel.OnCreateQuizPage();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("CreateQuiz"), Times.Once);
        }

        [Fact]
        public async Task OnEditQuiz_ShouldNavigateToCreateQuiz_WithParams()
        {
            // Arrange
            var quiz = CreateMockQuiz();

            // Act
            await _viewModel.OnEditQuiz(quiz);

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync(nameof(CreateQuiz), It.Is<Dictionary<string, object>>(d => d.ContainsKey("quizModel") && d.ContainsKey("id") && (int)d["id"] == quiz.Id)), Times.Once);
        }

        [Fact]
        public async Task OnDeleteQuizAsync_ShouldCallRepository()
        {
            // Arrange
            var quiz = CreateMockQuiz();

            // Act
            await _viewModel.OnDeleteQuizAsync(quiz);

            // Assert
            _quizzesRepoMock.Verify(x => x.DeleteQuiz(quiz.Id), Times.Once);
        }

        [Fact]
        public async Task OnViewQuiz_ShouldNavigateToExaminerViewQuiz()
        {
            // Arrange
            var quiz = CreateMockQuiz();

            // Act
            await _viewModel.OnViewQuiz(quiz);

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync(nameof(ExaminerViewQuiz), It.Is<Dictionary<string, object>>(d => d.ContainsKey("quiz") && d["quiz"] == quiz)), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_ShouldLoadQuizzes()
        {
            // Arrange
            var quizzes = new List<ExaminerQuiz> { CreateMockQuiz() };
            _quizzesRepoMock.Setup(x => x.GetUserQuizzes()).ReturnsAsync(quizzes);

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            _viewModel.AllExaminerQuizzes.Should().HaveCount(1);
            _viewModel.AllExaminerQuizzes.First().Id.Should().Be(1);
        }
    }
}
