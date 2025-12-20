using CodeQuizBackend.Authentication.Controllers;
using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Authentication.Services;
using CodeQuizBackend.Core.Data.models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CodeQuizBackend.Tests.Authentication.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly AuthenticationController _controller;

        public AuthenticationControllerTests()
        {
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _controller = new AuthenticationController(_authenticationServiceMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerModel = new RegisterModel { Username = "test", Password = "password", Email = "test@test.com", FirstName = "Test", LastName = "User" };
            var userDto = new UserDTO
            {
                Id = "user1",
                UserName = "test",
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                JoinDate = DateTime.Now
            };

            _authenticationServiceMock.Setup(x => x.Register(registerModel)).ReturnsAsync(userDto);

            // Act
            var result = await _controller.Register(registerModel);

            // Assert
            var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = actionResult.Value.Should().BeOfType<ApiResponse<UserDTO>>().Subject;
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().Be(userDto);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var loginModel = new LoginModel { Username = "test", Password = "password" };
            var loginResult = new LoginResult
            {
                User = new UserDTO
                {
                    Id = "user1",
                    UserName = "test",
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User",
                    JoinDate = DateTime.Now
                },
                TokenModel = new TokenModel { AccessToken = "access", RefreshToken = "refresh" }
            };

            _authenticationServiceMock.Setup(x => x.Login(loginModel)).ReturnsAsync(loginResult);

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = actionResult.Value.Should().BeOfType<ApiResponse<LoginResult>>().Subject;
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().Be(loginResult);
        }

        [Fact]
        public async Task Refresh_ShouldReturnOk_WhenRefreshIsSuccessful()
        {
            // Arrange
            var tokenModel = new TokenModel { AccessToken = "old_access", RefreshToken = "old_refresh" };
            var newTokenModel = new TokenModel { AccessToken = "new_access", RefreshToken = "new_refresh" };

            _authenticationServiceMock.Setup(x => x.RefreshToken(tokenModel)).ReturnsAsync(newTokenModel);

            // Act
            var result = await _controller.Refresh(tokenModel);

            // Assert
            var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = actionResult.Value.Should().BeOfType<ApiResponse<TokenModel>>().Subject;
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().Be(newTokenModel);
        }
    }
}
