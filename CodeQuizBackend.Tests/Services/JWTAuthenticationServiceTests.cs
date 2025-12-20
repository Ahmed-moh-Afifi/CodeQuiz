using CodeQuizBackend.Authentication.Exceptions;
using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Authentication.Services;
using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace CodeQuizBackend.Tests.Services
{
    public class JWTAuthenticationServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IMailService> _mailServiceMock;
        private readonly JWTAuthenticationService _jwtAuthenticationService;

        public JWTAuthenticationServiceTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            _tokenServiceMock = new Mock<ITokenService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _configurationMock = new Mock<IConfiguration>();
            _mailServiceMock = new Mock<IMailService>();

            _jwtAuthenticationService = new JWTAuthenticationService(
                _userManagerMock.Object,
                _tokenServiceMock.Object,
                _dbContext,
                _configurationMock.Object,
                _mailServiceMock.Object
            );
        }

        [Fact]
        public async Task Login_ShouldReturnLoginResult_WhenCredentialsAreValid()
        {
            // Arrange
            var loginModel = new LoginModel { Username = "testuser", Password = "Password123!" };
            var user = new User
            {
                Id = "user1",
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(loginModel.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginModel.Password)).ReturnsAsync(true);

            var accessToken = "access_token";
            var refreshToken = "refresh_token";
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>())).Returns(accessToken);
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);

            _configurationMock.Setup(x => x["RefreshTokenExpiresInDays"]).Returns("7");

            // Act
            var result = await _jwtAuthenticationService.Login(loginModel);

            // Assert
            result.Should().NotBeNull();
            result.User.UserName.Should().Be(user.UserName);
            result.TokenModel.AccessToken.Should().Be(accessToken);
            result.TokenModel.RefreshToken.Should().Be(refreshToken);
        }

        [Fact]
        public async Task Login_ShouldThrowInvalidCredentialsException_WhenUserNotFound()
        {
            // Arrange
            var loginModel = new LoginModel { Username = "testuser", Password = "Password123!" };
            _userManagerMock.Setup(x => x.FindByNameAsync(loginModel.Username)).ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await _jwtAuthenticationService.Login(loginModel);

            // Assert
            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [Fact]
        public async Task Login_ShouldThrowInvalidCredentialsException_WhenPasswordIsInvalid()
        {
            // Arrange
            var loginModel = new LoginModel { Username = "testuser", Password = "Password123!" };
            var user = new User
            {
                Id = "user1",
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(loginModel.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginModel.Password)).ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _jwtAuthenticationService.Login(loginModel);

            // Assert
            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [Fact]
        public async Task ForgetPassword_ShouldSendEmail_WhenUserExists()
        {
            // Arrange
            var forgetPasswordModel = new ForgetPasswordModel { Email = "test@example.com" };
            var user = new User
            {
                Id = "user1",
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(forgetPasswordModel.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset_token");
            _configurationMock.Setup(x => x["ForgetPasswordWebsiteUrl"]).Returns("http://example.com/reset?token=");

            // Act
            await _jwtAuthenticationService.ForgetPassword(forgetPasswordModel);

            // Assert
            _mailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(user.Email, user.FirstName, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgetPassword_ShouldNotSendEmail_WhenUserNotFound()
        {
            // Arrange
            var forgetPasswordModel = new ForgetPasswordModel { Email = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(forgetPasswordModel.Email)).ReturnsAsync((User)null);

            // Act
            await _jwtAuthenticationService.ForgetPassword(forgetPasswordModel);

            // Assert
            _mailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
