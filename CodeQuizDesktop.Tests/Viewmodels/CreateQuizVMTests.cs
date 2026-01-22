using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using FluentAssertions;
using Moq;
using System.Collections.ObjectModel;
using Xunit;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class CreateQuizVMTests
    {
        private readonly Mock<IQuizDialogService> _quizDialogServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IAuthenticationRepository> _authenticationRepositoryMock;
        private readonly Mock<IQuizzesRepository> _quizzesRepositoryMock;
        private readonly Mock<IExecutionRepository> _executionRepositoryMock;
        private readonly Mock<IUIService> _uiServiceMock;
        private readonly CreateQuizVM _viewModel;

        public CreateQuizVMTests()
        {
            _quizDialogServiceMock = new Mock<IQuizDialogService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            _quizzesRepositoryMock = new Mock<IQuizzesRepository>();
            _executionRepositoryMock = new Mock<IExecutionRepository>();
            _uiServiceMock = new Mock<IUIService>();

            _executionRepositoryMock.Setup(x => x.GetSupportedLanguages())
                .ReturnsAsync(new List<SupportedLanguage>
                {
                    new SupportedLanguage { Name = "Python", Extension = ".py" },
                    new SupportedLanguage { Name = "C#", Extension = ".cs" }
                });

            _viewModel = new CreateQuizVM(
                _quizDialogServiceMock.Object,
                _navigationServiceMock.Object,
                _authenticationRepositoryMock.Object,
                _quizzesRepositoryMock.Object,
                _executionRepositoryMock.Object,
                _uiServiceMock.Object
            );
        }

        [Fact]
        public async Task LoadProgrammingLanguages_ShouldPopulateLanguages()
        {
            // Act
            await _viewModel.LoadProgrammingLanguages();

            // Assert
            _viewModel.ProgrammingLanguages.Should().Contain(l => l.Name == "Python");
            _viewModel.ProgrammingLanguages.Should().Contain(l => l.Name == "C#");
        }

        [Fact]
        public async Task ReturnToPreviousPage_WhenNoChanges_ShouldNavigateBack()
        {
            // Arrange - no changes made (empty questions and no title)
            _viewModel.QuizTitle = null;
            _viewModel.QuestionModels.Clear();

            // Act
            await _viewModel.ReturnToPreviousPage();

            // Assert
            _navigationServiceMock.Verify(x => x.GoToAsync(".."), Times.Once);
        }

        [Fact]
        public async Task ReturnToPreviousPage_WhenChangesExist_ShouldConfirmFirst()
        {
            // Arrange - has unsaved changes
            _viewModel.QuizTitle = "Test Quiz";
            _uiServiceMock.Setup(x => x.ShowConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            await _viewModel.ReturnToPreviousPage();

            // Assert
            _uiServiceMock.Verify(x => x.ShowConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _navigationServiceMock.Verify(x => x.GoToAsync(".."), Times.Once);
        }

        [Fact]
        public async Task AddQuestion_ShouldAddQuestion_WhenDialogReturnsResult()
        {
            // Arrange
            var newQuestion = new NewQuestionModel
            {
                Statement = "Test Question",
                EditorCode = "print('hello')",
                TestCases = new List<TestCase>(),
                Order = 1,
                Points = 10
            };
            _quizDialogServiceMock.Setup(x => x.ShowAddQuestionDialogAsync(It.IsAny<string?>()))
                .ReturnsAsync(newQuestion);

            // Act
            await _viewModel.AddQuestion();

            // Assert
            _viewModel.QuestionModels.Should().Contain(newQuestion);
        }

        [Fact]
        public async Task AddQuestion_ShouldNotAddQuestion_WhenDialogReturnsNull()
        {
            // Arrange
            _quizDialogServiceMock.Setup(x => x.ShowAddQuestionDialogAsync(It.IsAny<string?>()))
                .ReturnsAsync((NewQuestionModel?)null);

            // Act
            await _viewModel.AddQuestion();

            // Assert
            _viewModel.QuestionModels.Should().BeEmpty();
        }

        [Fact]
        public async Task EditQuestion_ShouldUpdateQuestion_WhenDialogReturnsResult()
        {
            // Arrange
            var originalQuestion = new NewQuestionModel
            {
                Statement = "Original",
                EditorCode = "code",
                TestCases = new List<TestCase>(),
                Order = 1,
                Points = 10
            };
            _viewModel.QuestionModels.Add(originalQuestion);

            var updatedQuestion = new NewQuestionModel
            {
                Statement = "Updated",
                EditorCode = "code",
                TestCases = new List<TestCase>(),
                Order = 1,
                Points = 20
            };

            _quizDialogServiceMock.Setup(x => x.ShowEditQuestionDialogAsync(originalQuestion, It.IsAny<string?>()))
                .ReturnsAsync(updatedQuestion);

            // Act
            await _viewModel.EditQuestion(originalQuestion);

            // Assert
            _viewModel.QuestionModels.Should().Contain(updatedQuestion);
            _viewModel.QuestionModels.Should().NotContain(originalQuestion);
            _viewModel.QuestionModels.First().Statement.Should().Be("Updated");
        }

        [Fact]
        public async Task DeleteQuestionAsync_WhenConfirmed_ShouldRemoveQuestion()
        {
            // Arrange
            var question = new NewQuestionModel
            {
                Statement = "Test",
                EditorCode = "code",
                TestCases = new List<TestCase>(),
                Order = 1,
                Points = 10
            };
            _viewModel.QuestionModels.Add(question);

            _uiServiceMock.Setup(x => x.ShowDestructiveConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            await _viewModel.DeleteQuestionAsync(question);

            // Assert
            _viewModel.QuestionModels.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteQuestionAsync_WhenCancelled_ShouldNotRemoveQuestion()
        {
            // Arrange
            var question = new NewQuestionModel
            {
                Statement = "Test",
                EditorCode = "code",
                TestCases = new List<TestCase>(),
                Order = 1,
                Points = 10
            };
            _viewModel.QuestionModels.Add(question);

            _uiServiceMock.Setup(x => x.ShowDestructiveConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            await _viewModel.DeleteQuestionAsync(question);

            // Assert
            _viewModel.QuestionModels.Should().Contain(question);
        }

        [Fact]
        public void ValidateUserInput_ShouldReturnErrors_WhenInputIsInvalid()
        {
            // Arrange
            _viewModel.QuizTitle = ""; // Invalid
            _viewModel.QuizDurationInMinutes = "invalid"; // Invalid
            _viewModel.ProgrammingLanguage = ""; // Invalid
            _viewModel.QuestionModels.Clear(); // Invalid

            // Act
            var errors = _viewModel.ValidateUserInput();

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain("Quiz title cannot be empty");
            errors.Should().Contain("Invalid duration value");
            errors.Should().Contain("Programming language not selected");
            errors.Should().Contain("Cannot create quiz without questions");
        }

        [Fact]
        public async Task CreateAndPublishQuizAsync_ShouldCreateQuiz_WhenInputIsValid()
        {
            // Arrange
            _viewModel.QuizTitle = "Valid Quiz";
            _viewModel.QuizDurationInMinutes = "60";
            _viewModel.ProgrammingLanguage = "Python";
            _viewModel.AvailableFromDate = DateTime.Now;
            _viewModel.AvailableToDate = DateTime.Now.AddDays(1);

            var question = new NewQuestionModel
            {
                Statement = "Q1",
                EditorCode = "code",
                TestCases = new List<TestCase>(),
                Order = 1,
                Points = 10
            };
            _viewModel.QuestionModels.Add(question);

            var user = new User
            {
                Id = "1",
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
                JoinDate = DateTime.Now
            };
            _authenticationRepositoryMock.Setup(x => x.LoggedInUser).Returns(user);

            _uiServiceMock.Setup(x => x.ShowConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            await _viewModel.CreateAndPublishQuizAsync();

            // Assert
            _quizzesRepositoryMock.Verify(x => x.CreateQuiz(It.IsAny<NewQuizModel>()), Times.Once);
        }
    }
}
