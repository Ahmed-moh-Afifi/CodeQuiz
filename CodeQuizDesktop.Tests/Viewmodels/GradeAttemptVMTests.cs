using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class GradeAttemptVMTests
    {
        private readonly Mock<IExecutionRepository> _executionRepoMock;
        private readonly Mock<IAttemptsRepository> _attemptsRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly Mock<IAuthenticationRepository> _authRepoMock;
        private readonly Mock<IUIService> _uiServiceMock;
        private readonly GradeAttemptVM _viewModel;

        public GradeAttemptVMTests()
        {
            _executionRepoMock = new Mock<IExecutionRepository>();
            _attemptsRepoMock = new Mock<IAttemptsRepository>();
            _navServiceMock = new Mock<INavigationService>();
            _authRepoMock = new Mock<IAuthenticationRepository>();
            _uiServiceMock = new Mock<IUIService>();

            // Setup auth repo to return a mock user
            _authRepoMock.Setup(x => x.LoggedInUser).Returns(new User
            {
                Id = "instructor1",
                UserName = "instructor",
                FirstName = "Test",
                LastName = "Instructor",
                Email = "instructor@example.com",
                JoinDate = DateTime.Now
            });

            _viewModel = new GradeAttemptVM(_executionRepoMock.Object, _attemptsRepoMock.Object, _navServiceMock.Object, _authRepoMock.Object, _uiServiceMock.Object);
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
                Questions = new List<Question>
                {
                    new Question
                    {
                        Id = 101,
                        Statement = "Q1",
                        EditorCode = "code1",
                        QuestionConfiguration = new QuestionConfiguration
                        {
                            Language = "csharp",
                            AllowExecution = true,
                            ShowOutput = true,
                            ShowError = true
                        },
                        TestCases = new List<TestCase>(),
                        QuizId = 1,
                        Order = 1,
                        Points = 10
                    },
                    new Question
                    {
                        Id = 102,
                        Statement = "Q2",
                        EditorCode = "code2",
                        QuestionConfiguration = new QuestionConfiguration
                        {
                            Language = "csharp",
                            AllowExecution = true,
                            ShowOutput = true,
                            ShowError = true
                        },
                        TestCases = new List<TestCase>(),
                        QuizId = 1,
                        Order = 2,
                        Points = 10
                    }
                },
                QustionsCount = 2,
                AttemptsCount = 0,
                SubmittedAttemptsCount = 0,
                AverageAttemptScore = 0,
                TotalPoints = 20
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
                Solutions = new List<Solution>
                {
                    new Solution { Id = 201, Code = "sol1", QuestionId = 101, AttemptId = 1, ReceivedGrade = 5 },
                    new Solution { Id = 202, Code = "sol2", QuestionId = 102, AttemptId = 1, ReceivedGrade = 8 }
                },
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

        private void SetupViewModel()
        {
            var quiz = CreateMockQuiz();
            var attempt = CreateMockAttempt();
            _viewModel.Quiz = quiz;
            _viewModel.Attempt = attempt;
            _viewModel.SelectedQuestion = quiz.Questions[0];
            _viewModel.Grade = attempt.Solutions[0].ReceivedGrade;
        }

        [Fact]
        public async Task ReturnToPreviousPage_WithNoChanges_ShouldNavigateBackWithoutSaving()
        {
            // Arrange
            SetupViewModel();
            // No grade changes made

            // Act
            await _viewModel.ReturnToPreviousPage();

            // Assert - no save should occur, just navigate back
            _attemptsRepoMock.Verify(x => x.BatchUpdateSolutionGrades(It.IsAny<BatchUpdateSolutionGradesRequest>()), Times.Never);
            _navServiceMock.Verify(x => x.GoToAsync(".."), Times.Once);
        }

        [Fact]
        public void NextQuestion_ShouldMoveToNextWithoutSaving()
        {
            // Arrange
            SetupViewModel();
            _viewModel.SelectedQuestion.Order.Should().Be(1);

            // Act
            _viewModel.NextQuestion();

            // Assert - no save should occur, just move to next question
            _attemptsRepoMock.Verify(x => x.BatchUpdateSolutionGrades(It.IsAny<BatchUpdateSolutionGradesRequest>()), Times.Never);
            _viewModel.SelectedQuestion.Order.Should().Be(2);
            _viewModel.Grade.Should().Be(8); // Grade for Q2
        }

        [Fact]
        public void PreviousQuestion_ShouldMoveToPreviousWithoutSaving()
        {
            // Arrange
            SetupViewModel();
            _viewModel.NextQuestion(); // Move to 2
            _viewModel.SelectedQuestion.Order.Should().Be(2);

            // Act
            _viewModel.PreviousQuestion();

            // Assert - no save should occur, just move to previous question
            _attemptsRepoMock.Verify(x => x.BatchUpdateSolutionGrades(It.IsAny<BatchUpdateSolutionGradesRequest>()), Times.Never);
            _viewModel.SelectedQuestion.Order.Should().Be(1);
            _viewModel.Grade.Should().Be(5); // Grade for Q1
        }

        [Fact]
        public async Task SaveAllGradesAndGoBack_ShouldSaveOnlyChangedGrades()
        {
            // Arrange
            SetupViewModel();

            // Simulate ApplyQueryAttributes to initialize tracking dictionaries
            _viewModel.ApplyQueryAttributes(new Dictionary<string, object>
            {
                { "attempt", _viewModel.Attempt },
                { "quiz", _viewModel.Quiz }
            });

            // Now change the grade from 5 to 10
            _viewModel.Grade = 10;

            // Act
            await _viewModel.SaveAllGradesAndGoBack();

            // Assert - should save only the changed grade (solution ID 201) via batch update
            _attemptsRepoMock.Verify(x => x.BatchUpdateSolutionGrades(It.Is<BatchUpdateSolutionGradesRequest>(r =>
                r.AttemptId == 1 &&
                r.Updates.Count == 1 &&
                r.Updates.Any(u => u.SolutionId == 201 && u.ReceivedGrade == 10))), Times.Once);
            _navServiceMock.Verify(x => x.GoToAsync(".."), Times.Once);
        }

        [Fact]
        public async Task Run_ShouldExecuteCode()
        {
            // Arrange
            SetupViewModel();
            _viewModel.CodeInEditor = "Console.WriteLine(\"Hello\");";
            _executionRepoMock.Setup(x => x.RunCode(It.IsAny<RunCodeRequest>()))
                .ReturnsAsync(new CodeRunnerResult { Success = true, Output = "Hello" });

            // Act
            await _viewModel.Run();

            // Assert
            _executionRepoMock.Verify(x => x.RunCode(It.Is<RunCodeRequest>(r => r.Code == "Console.WriteLine(\"Hello\");")), Times.Once);
            _viewModel.Output.Should().Be("Hello");
        }
    }
}
