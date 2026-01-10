using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using FluentAssertions;
using Moq;
using System.Collections.ObjectModel;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class ExaminerViewQuizVMTests
    {
        private readonly Mock<IQuizzesRepository> _quizzesRepoMock;
        private readonly Mock<IAttemptsRepository> _attemptsRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly ExaminerViewQuizVM _viewModel;

        public ExaminerViewQuizVMTests()
        {
            _quizzesRepoMock = new Mock<IQuizzesRepository>();
            _attemptsRepoMock = new Mock<IAttemptsRepository>();
            _navServiceMock = new Mock<INavigationService>();
            _viewModel = new ExaminerViewQuizVM(_quizzesRepoMock.Object, _attemptsRepoMock.Object, _navServiceMock.Object);
        }

        private ExaminerQuiz CreateMockQuiz()
        {
            return new ExaminerQuiz
            {
                Id = 1,
                Title = "Test Quiz",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Duration = TimeSpan.FromMinutes(60),
                Code = "TESTCODE",
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
        }

        private ExaminerAttempt CreateMockAttempt()
        {
            return new ExaminerAttempt
            {
                Id = 1,
                StartTime = DateTime.Now,
                QuizId = 1,
                ExamineeId = "examinee1",
                Solutions = new List<Solution>(),
                Examinee = new User
                {
                    Id = "examinee1",
                    UserName = "user1",
                    FirstName = "User",
                    LastName = "One",
                    Email = "user1@example.com",
                    JoinDate = DateTime.Now
                },
                TotalPoints = 100
            };
        }

        [Fact]
        public async Task ReturnToPreviousPage_ShouldNavigateBack()
        {
            // Arrange - Set a quiz so we can properly leave groups
            _viewModel.Quiz = CreateMockQuiz();
            
            // Act
            await _viewModel.ReturnToPreviousPage();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync(".."), Times.Once);
            _attemptsRepoMock.Verify(x => x.LeaveQuizGroupAsync(1), Times.Once);
        }

        [Fact]
        public async Task OnGoToGradeAttemptPage_ShouldNavigateToGradeAttempt()
        {
            // Arrange
            var attempt = CreateMockAttempt();
            var quiz = CreateMockQuiz();
            _viewModel.Quiz = quiz;

            // Act
            await _viewModel.OnGoToGradeAttemptPage(attempt);

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("GradeAttempt", It.Is<Dictionary<string, object>>(d =>
                d.ContainsKey("attempt") && d["attempt"] == attempt &&
                d.ContainsKey("quiz") && d["quiz"] == quiz
            )), Times.Once);
        }

        [Fact]
        public async Task LoadAttemptsAsync_ShouldPopulateAttempts()
        {
            // Arrange
            var quiz = CreateMockQuiz();
            _viewModel.Quiz = quiz;
            var attempts = new List<ExaminerAttempt> { CreateMockAttempt() };
            _quizzesRepoMock.Setup(x => x.GetQuizAttempts(quiz.Id)).ReturnsAsync(attempts);

            // Act
            await _viewModel.LoadAttemptsAsync();

            // Assert
            _viewModel.Attempts.Should().HaveCount(1);
            _viewModel.Attempts.First().Should().BeEquivalentTo(attempts.First());
        }
    }
}
