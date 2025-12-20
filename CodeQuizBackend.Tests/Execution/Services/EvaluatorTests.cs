using CodeQuizBackend.Execution.Models;
using CodeQuizBackend.Execution.Services;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CodeQuizBackend.Tests.Execution.Services
{
    public class EvaluatorTests
    {
        private readonly Mock<ICodeRunnerFactory> _codeRunnerFactoryMock;
        private readonly Evaluator _evaluator;

        public EvaluatorTests()
        {
            _codeRunnerFactoryMock = new Mock<ICodeRunnerFactory>();
            _evaluator = new Evaluator(_codeRunnerFactoryMock.Object);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnSuccessfulResult_WhenOutputMatchesExpected()
        {
            // Arrange
            var language = "csharp";
            var code = "Console.WriteLine(\"Hello World\");";
            var testCase = new TestCase { TestCaseNumber = 1, Input = new List<string>(), ExpectedOutput = "Hello World" };

            var codeRunnerMock = new Mock<ICodeRunner>();
            codeRunnerMock.Setup(x => x.RunCodeAsync(code, It.IsAny<CodeRunnerOptions>()))
                .ReturnsAsync(new CodeRunnerResult { Success = true, Output = "Hello World" });

            _codeRunnerFactoryMock.Setup(x => x.Create(language, true)).Returns(codeRunnerMock.Object);

            // Act
            var result = await _evaluator.EvaluateAsync(language, code, testCase);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Output.Should().Be("Hello World");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFailureResult_WhenOutputDoesNotMatchExpected()
        {
            // Arrange
            var language = "csharp";
            var code = "Console.WriteLine(\"Wrong Output\");";
            var testCase = new TestCase { TestCaseNumber = 1, Input = new List<string>(), ExpectedOutput = "Hello World" };

            var codeRunnerMock = new Mock<ICodeRunner>();
            codeRunnerMock.Setup(x => x.RunCodeAsync(code, It.IsAny<CodeRunnerOptions>()))
                .ReturnsAsync(new CodeRunnerResult { Success = true, Output = "Wrong Output" });

            _codeRunnerFactoryMock.Setup(x => x.Create(language, true)).Returns(codeRunnerMock.Object);

            // Act
            var result = await _evaluator.EvaluateAsync(language, code, testCase);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Output.Should().Be("Wrong Output");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFailureResult_WhenExecutionFails()
        {
            // Arrange
            var language = "csharp";
            var code = "Invalid Code";
            var testCase = new TestCase { TestCaseNumber = 1, Input = new List<string>(), ExpectedOutput = "Hello World" };

            var codeRunnerMock = new Mock<ICodeRunner>();
            codeRunnerMock.Setup(x => x.RunCodeAsync(code, It.IsAny<CodeRunnerOptions>()))
                .ReturnsAsync(new CodeRunnerResult { Success = false, Output = "Compilation Error" });

            _codeRunnerFactoryMock.Setup(x => x.Create(language, true)).Returns(codeRunnerMock.Object);

            // Act
            var result = await _evaluator.EvaluateAsync(language, code, testCase);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Output.Should().Be("Compilation Error");
        }
    }
}
