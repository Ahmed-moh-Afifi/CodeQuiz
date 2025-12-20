using CodeQuizDesktop.Models;
using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class LoginVMTests
    {
        private readonly Mock<IAuthenticationRepository> _authenticationRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly LoginVM _loginVM;

        public LoginVMTests()
        {
            _authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _navigationServiceMock = new Mock<INavigationService>();

            _loginVM = new LoginVM(
                _authenticationRepositoryMock.Object,
                _tokenServiceMock.Object,
                _navigationServiceMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ShouldCallLoginAndNavigate_WhenCredentialsAreValid()
        {
            // Arrange
            _loginVM.Username = "testuser";
            _loginVM.Password = "password";

            var loginResult = new LoginResult
            {
                User = new User
                {
                    Id = "1",
                    UserName = "testuser",
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@test.com",
                    JoinDate = DateTime.Now
                },
                TokenModel = new TokenModel { AccessToken = "access", RefreshToken = "refresh" }
            };

            _authenticationRepositoryMock.Setup(x => x.Login(It.IsAny<LoginModel>()))
                .ReturnsAsync(loginResult);

            // Act
            await _loginVM.LoginAsync();

            // Assert
            _authenticationRepositoryMock.Verify(x => x.Login(It.Is<LoginModel>(m => m.Username == "testuser" && m.Password == "password")), Times.Once);
            _navigationServiceMock.Verify(x => x.GoToAsync("///MainPage"), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldNotCallLogin_WhenUsernameIsEmpty()
        {
            // Arrange
            _loginVM.Username = "";
            _loginVM.Password = "password";

            // Act
            await _loginVM.LoginAsync();

            // Assert
            _authenticationRepositoryMock.Verify(x => x.Login(It.IsAny<LoginModel>()), Times.Never);
            _navigationServiceMock.Verify(x => x.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task OpenRegisterPageAsync_ShouldNavigateToRegisterPage()
        {
            // Act
            await _loginVM.OpenRegisterPageAsync();

            // Assert
            _navigationServiceMock.Verify(x => x.GoToAsync("///RegisterPage"), Times.Once);
        }
    }
}
