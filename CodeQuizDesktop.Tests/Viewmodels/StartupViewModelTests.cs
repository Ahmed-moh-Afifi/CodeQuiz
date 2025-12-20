using CodeQuizDesktop.Models;
using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using FluentAssertions;
using Moq;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class StartupViewModelTests
    {
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IAuthenticationRepository> _authRepoMock;
        private readonly Mock<IUsersRepository> _usersRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly StartupViewModel _viewModel;

        public StartupViewModelTests()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _authRepoMock = new Mock<IAuthenticationRepository>();
            _usersRepoMock = new Mock<IUsersRepository>();
            _navServiceMock = new Mock<INavigationService>();

            _viewModel = new StartupViewModel(_tokenServiceMock.Object, _authRepoMock.Object, _usersRepoMock.Object, _navServiceMock.Object);
        }

        [Fact]
        public async Task InitializeAsync_ShouldNavigateToMainPage_WhenTokenIsValid()
        {
            // Arrange
            _tokenServiceMock.Setup(x => x.GetValidTokens()).ReturnsAsync(new TokenModel { AccessToken = "access_token", RefreshToken = "refresh_token" });
            _usersRepoMock.Setup(x => x.GetUser()).ReturnsAsync(new User { Id = "1", FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test", JoinDate = DateTime.Now });

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("///MainPage"), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_ShouldNavigateToLoginPage_WhenTokenIsInvalid()
        {
            // Arrange
            _tokenServiceMock.Setup(x => x.GetValidTokens()).ReturnsAsync((TokenModel?)null);

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("///LoginPage"), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_ShouldNavigateToLoginPage_WhenExceptionOccurs()
        {
            // Arrange
            _tokenServiceMock.Setup(x => x.GetValidTokens()).ThrowsAsync(new Exception("Connection failed"));

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("///LoginPage"), Times.Once);
        }
    }
}