using CodeQuizDesktop.Models;
using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using CodeQuizDesktop.Viewmodels;
using FluentAssertions;
using Moq;

namespace CodeQuizDesktop.Tests.Viewmodels
{
    public class JoinQuizVMTests
    {
        private readonly Mock<IAttemptsRepository> _attemptsRepoMock;
        private readonly Mock<IExecutionRepository> _executionRepoMock;
        private readonly Mock<INavigationService> _navServiceMock;
        private readonly Mock<IUIService> _uiServiceMock;
        private readonly JoinQuizVM _viewModel;

        public JoinQuizVMTests()
        {
            _attemptsRepoMock = new Mock<IAttemptsRepository>();
            _executionRepoMock = new Mock<IExecutionRepository>();
            _navServiceMock = new Mock<INavigationService>();
            _uiServiceMock = new Mock<IUIService>();

            _viewModel = new JoinQuizVM(_attemptsRepoMock.Object, _executionRepoMock.Object, _navServiceMock.Object, _uiServiceMock.Object);
        }

        [Fact]
        public async Task ReturnToPreviousPageAsync_ShouldNavigateToMainPage()
        {
            // Act
            await _viewModel.ReturnToPreviousPageAsync();

            // Assert
            _navServiceMock.Verify(x => x.GoToAsync("///MainPage"), Times.Once);
        }
    }
}