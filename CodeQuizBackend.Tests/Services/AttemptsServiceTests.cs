using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Services;
using CodeQuizBackend.Execution.Services;
using CodeQuizBackend.Quiz.Exceptions;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Models;
using CodeQuizBackend.Quiz.Services;
using CodeQuizBackend.Services;
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
    public class AttemptsServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IEvaluator> _evaluatorMock;
        private readonly Mock<IHubContext<AttemptsHub>> _attemptsHubContextMock;
        private readonly Mock<IHubContext<QuizzesHub>> _quizzesHubContextMock;
        private readonly Mock<IMailService> _mailServiceMock;
        private readonly Mock<IEvaluationQueue> _evaluationQueueMock;
        private readonly AttemptsService _attemptsService;

        public AttemptsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _evaluatorMock = new Mock<IEvaluator>();

            _attemptsHubContextMock = new Mock<IHubContext<AttemptsHub>>();
            var attemptsClientsMock = new Mock<IHubClients>();
            var attemptsClientProxyMock = new Mock<IClientProxy>();
            _attemptsHubContextMock.Setup(x => x.Clients).Returns(attemptsClientsMock.Object);
            attemptsClientsMock.Setup(x => x.All).Returns(attemptsClientProxyMock.Object);
            attemptsClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(attemptsClientProxyMock.Object);

            _quizzesHubContextMock = new Mock<IHubContext<QuizzesHub>>();
            var quizzesClientsMock = new Mock<IHubClients>();
            var quizzesClientProxyMock = new Mock<IClientProxy>();
            _quizzesHubContextMock.Setup(x => x.Clients).Returns(quizzesClientsMock.Object);
            quizzesClientsMock.Setup(x => x.All).Returns(quizzesClientProxyMock.Object);
            quizzesClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(quizzesClientProxyMock.Object);

            _mailServiceMock = new Mock<IMailService>();
            _evaluationQueueMock = new Mock<IEvaluationQueue>();

            _attemptsService = new AttemptsService(
                _dbContext,
                _evaluatorMock.Object,
                _attemptsHubContextMock.Object,
                _quizzesHubContextMock.Object,
                _mailServiceMock.Object,
                _evaluationQueueMock.Object
            );
        }

        [Fact]
        public async Task BeginAttempt_ShouldCreateNewAttempt_WhenNoActiveAttemptExists()
        {
            // Arrange
            var quizCode = "ABCDEF";
            var examineeId = "examinee1";
            var quiz = new CodeQuizBackend.Quiz.Models.Quiz
            {
                Id = 1,
                Code = quizCode,
                Title = "Test Quiz",
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1),
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

            var examinee = new CodeQuizBackend.Authentication.Models.User
            {
                Id = examineeId,
                FirstName = "Jane",
                LastName = "Doe",
                UserName = "janedoe",
                Email = "jane@example.com"
            };
            _dbContext.Users.Add(examinee);

            _dbContext.Quizzes.Add(quiz);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _attemptsService.BeginAttempt(quizCode, examineeId);

            // Assert
            result.Should().NotBeNull();
            result.Quiz.Code.Should().Be(quizCode);
            var attempt = await _dbContext.Attempts.FirstOrDefaultAsync(a => a.ExamineeId == examineeId && a.QuizId == quiz.Id);
            attempt.Should().NotBeNull();
        }

        [Fact]
        public async Task BeginAttempt_ShouldThrowQuizNotActiveException_WhenQuizHasNotStarted()
        {
            // Arrange
            var quizCode = "NOTSTARTED";
            var examineeId = "examinee1";
            var quiz = new CodeQuizBackend.Quiz.Models.Quiz
            {
                Id = 2,
                Code = quizCode,
                Title = "Future Quiz",
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

            _dbContext.Quizzes.Add(quiz);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _attemptsService.BeginAttempt(quizCode, examineeId);

            // Assert
            await act.Should().ThrowAsync<QuizNotActiveException>();
        }
    }
}
