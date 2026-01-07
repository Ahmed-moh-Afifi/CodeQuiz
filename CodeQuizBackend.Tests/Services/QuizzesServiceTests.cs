using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Models;
using CodeQuizBackend.Quiz.Models.DTOs;
using CodeQuizBackend.Quiz.Repositories;
using CodeQuizBackend.Quiz.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CodeQuizBackend.Tests.Services
{
    public class QuizzesServiceTests
    {
        private readonly Mock<IQuizzesRepository> _quizzesRepositoryMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IQuizCodeGenerator> _quizCodeGeneratorMock;
        private readonly Mock<IHubContext<QuizzesHub>> _quizzesHubContextMock;
        private readonly Mock<IAppLogger<QuizzesService>> _loggerMock;
        private readonly QuizzesService _quizzesService;

        public QuizzesServiceTests()
        {
            _quizzesRepositoryMock = new Mock<IQuizzesRepository>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _quizCodeGeneratorMock = new Mock<IQuizCodeGenerator>();
            _quizzesHubContextMock = new Mock<IHubContext<QuizzesHub>>();
            _loggerMock = new Mock<IAppLogger<QuizzesService>>();

            _quizzesService = new QuizzesService(
                _quizzesRepositoryMock.Object,
                _dbContext,
                _quizCodeGeneratorMock.Object,
                _quizzesHubContextMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateQuiz_ShouldReturnExaminerQuiz_WhenQuizIsCreatedSuccessfully()
        {
            // Arrange
            var newQuizModel = new NewQuizModel
            {
                Title = "Test Quiz",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(2),
                Duration = TimeSpan.FromMinutes(60),
                ExaminerId = "examiner1",
                GlobalQuestionConfiguration = new QuestionConfiguration
                {
                    Language = "csharp",
                    AllowExecution = true,
                    ShowOutput = true,
                    ShowError = true
                },
                AllowMultipleAttempts = false,
                Questions = new List<NewQuestionModel>()
            };

            var generatedCode = "ABCDEF";
            _quizCodeGeneratorMock.Setup(x => x.GenerateUniqueQuizCode()).ReturnsAsync(generatedCode);

            var createdQuiz = newQuizModel.ToQuiz(generatedCode);
            createdQuiz.Id = 1;

            _quizzesRepositoryMock.Setup(x => x.CreateQuizAsync(It.IsAny<CodeQuizBackend.Quiz.Models.Quiz>()))
                .ReturnsAsync(createdQuiz);

            // Act
            var result = await _quizzesService.CreateQuiz(newQuizModel);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(generatedCode);
            result.Title.Should().Be(newQuizModel.Title);
            _quizzesRepositoryMock.Verify(x => x.CreateQuizAsync(It.Is<CodeQuizBackend.Quiz.Models.Quiz>(q => q.Code == generatedCode)), Times.Once);
        }

        [Fact]
        public async Task DeleteQuiz_ShouldCallRepositoryAndDeleteQuiz()
        {
            // Arrange
            var quizId = 1;
            var examinerId = "examiner1";
            var quiz = new CodeQuizBackend.Quiz.Models.Quiz
            {
                Id = quizId,
                Title = "Test Quiz",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Duration = TimeSpan.FromMinutes(60),
                Code = "TESTCODE",
                ExaminerId = examinerId,
                GlobalQuestionConfiguration = new QuestionConfiguration
                {
                    Language = "csharp",
                    AllowExecution = true,
                    ShowOutput = true,
                    ShowError = true
                },
                AllowMultipleAttempts = false,
                Questions = new List<Question>(),
                Examiner = new CodeQuizBackend.Authentication.Models.User
                {
                    Id = examinerId,
                    FirstName = "John",
                    LastName = "Doe",
                    UserName = "johndoe",
                    Email = "john@example.com"
                }
            };
            _dbContext.Quizzes.Add(quiz);
            await _dbContext.SaveChangesAsync();

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();

            _quizzesHubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(x => x.All).Returns(clientProxyMock.Object);
            clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

            // Act
            await _quizzesService.DeleteQuiz(quizId);

            // Assert
            _quizzesRepositoryMock.Verify(x => x.DeleteQuizAsync(quizId), Times.Once);
            clientProxyMock.Verify(x => x.SendCoreAsync("QuizDeleted", It.Is<object[]>(o => (int)o[0] == quizId), default), Times.Once);
        }

        [Fact]
        public async Task GetQuizByCode_ShouldReturnExamineeQuiz_WhenQuizExists()
        {
            // Arrange
            var code = "ABCDEF";
            var quiz = new CodeQuizBackend.Quiz.Models.Quiz
            {
                Id = 1,
                Code = code,
                Title = "Test Quiz",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(2),
                Duration = TimeSpan.FromMinutes(60),
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
                Examiner = new CodeQuizBackend.Authentication.Models.User
                {
                    Id = "examiner1",
                    FirstName = "John",
                    LastName = "Doe",
                    UserName = "johndoe",
                    Email = "john@example.com"
                }
            };

            _quizzesRepositoryMock.Setup(x => x.GetQuizByCodeAsync(code)).ReturnsAsync(quiz);

            // Act
            var result = await _quizzesService.GetQuizByCode(code);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(code);
            result.Title.Should().Be(quiz.Title);
        }
    }
}
