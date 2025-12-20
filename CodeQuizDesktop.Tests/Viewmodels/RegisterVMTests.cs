using CodeQuizDesktop.Models.Authentication;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using FluentAssertions;
using Moq;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class RegisterVMTests
    {
        private readonly Mock<IAuthenticationRepository> _authRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly RegisterVM _viewModel;

        public RegisterVMTests()
        {
            _authRepoMock = new Mock<IAuthenticationRepository>();
            _navServiceMock = new Mock<INavigationService>();
            _viewModel = new RegisterVM(_authRepoMock.Object, _navServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldNotCallRepository_WhenFieldsAreEmpty()
        {
            // Arrange
            _viewModel.FirstName = "";
            _viewModel.LastName = "";
            _viewModel.Email = "";
            _viewModel.Username = "";
            _viewModel.Password = "";

            // Act
            await _viewModel.RegisterAsync();

            // Assert
            _authRepoMock.Verify(x => x.Register(It.IsAny<RegisterModel>()), Times.Never);
            _navServiceMock.Verify(x => x.GoToAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCallRepositoryAndNavigate_WhenFieldsAreValid()
        {
            // Arrange
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";
            _viewModel.Email = "john@example.com";
            _viewModel.Username = "johndoe";
            _viewModel.Password = "password123";

            _authRepoMock.Setup(x => x.Register(It.IsAny<RegisterModel>()))
                .ReturnsAsync(new User
                {
                    Id = "1",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com",
                    UserName = "johndoe",
                    JoinDate = DateTime.Now
                });

            // Act
            await _viewModel.RegisterAsync();

            // Assert
            _authRepoMock.Verify(x => x.Register(It.Is<RegisterModel>(m =>
                m.FirstName == "John" &&
                m.LastName == "Doe" &&
                m.Email == "john@example.com" &&
                m.Username == "johndoe" &&
                m.Password == "password123"
            )), Times.Once);

            _navServiceMock.Verify(x => x.GoToAsync("///LoginPage"), Times.Once);
        }

        [Fact]
        public async Task OpenLoginPageAsync_ShouldNavigateToLoginPage()
        {
            // Act
            await _viewModel.OpenLoginPageAsync();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("///LoginPage"), Times.Once);
        }
    }
}