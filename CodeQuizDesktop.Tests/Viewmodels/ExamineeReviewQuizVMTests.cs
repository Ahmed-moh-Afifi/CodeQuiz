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
    public class ExamineeReviewQuizVMTests
    {
        private readonly Mock<IExecutionRepository> _executionRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly ExamineeReviewQuizVM _viewModel;

        public ExamineeReviewQuizVMTests()
        {
            _executionRepoMock = new Mock<IExecutionRepository>();
            _navServiceMock = new Mock<INavigationService>();
            _viewModel = new ExamineeReviewQuizVM(_executionRepoMock.Object, _navServiceMock.Object);
        }

        private ExamineeQuiz CreateMockQuiz()
        {
            return new ExamineeQuiz
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
                TotalPoints = 20,
                Examiner = new User
                {
                    Id = "examiner1",
                    UserName = "examiner1",
                    FirstName = "Examiner",
                    LastName = "One",
                    Email = "examiner1@example.com",
                    JoinDate = DateTime.Now
                }
            };
        }

        private ExamineeAttempt CreateMockAttempt()
        {
            var quiz = CreateMockQuiz();
            return new ExamineeAttempt
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
                Quiz = quiz,
                MaxEndTime = DateTime.Now.AddHours(1)
            };
        }

        private void SetupViewModel()
        {
            var attempt = CreateMockAttempt();
            _viewModel.Attempt = attempt;
            _viewModel.SelectedQuestion = attempt.Quiz.Questions[0];
            _viewModel.Grade = attempt.Solutions[0].ReceivedGrade;
        }

        [Fact]
        public async Task ReturnToPreviousPage_ShouldNavigateBack()
        {
            // Arrange
            SetupViewModel();

            // Act
            await _viewModel.ReturnToPreviousPage();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync(".."), Times.Once);
        }

        [Fact]
        public void NextQuestion_ShouldMoveToNext()
        {
            // Arrange
            SetupViewModel();
            _viewModel.SelectedQuestion.Order.Should().Be(1);

            // Act
            _viewModel.NextQuestion();

            // Assert
            _viewModel.SelectedQuestion.Order.Should().Be(2);
            _viewModel.Grade.Should().Be(8); // Grade for Q2
        }

        [Fact]
        public void PreviousQuestion_ShouldMoveToPrevious()
        {
            // Arrange
            SetupViewModel();
            _viewModel.NextQuestion(); // Move to 2
            _viewModel.SelectedQuestion.Order.Should().Be(2);

            // Act
            _viewModel.PreviousQuestion();

            // Assert
            _viewModel.SelectedQuestion.Order.Should().Be(1);
            _viewModel.Grade.Should().Be(5); // Grade for Q1
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
