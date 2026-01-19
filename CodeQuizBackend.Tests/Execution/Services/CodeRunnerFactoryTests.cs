using CodeQuizBackend.Execution.Exceptions;
using CodeQuizBackend.Execution.Services;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CodeQuizBackend.Tests.Execution.Services
{
    public class CodeRunnerFactoryTests
    {
        private readonly Mock<ICodeRunner> _csharpRunnerMock;
        private readonly Mock<ICodeRunner> _pythonRunnerMock;
        private readonly Mock<ICodeRunner> _sandboxedRunnerMock;
        private readonly SandboxedCodeRunnerFactory _sandboxFactory;
        private readonly CodeRunnerFactory _factory;

        public CodeRunnerFactoryTests()
        {
            _csharpRunnerMock = new Mock<ICodeRunner>();
            _csharpRunnerMock.Setup(x => x.Language).Returns("csharp");

            _pythonRunnerMock = new Mock<ICodeRunner>();
            _pythonRunnerMock.Setup(x => x.Language).Returns("python");

            _sandboxedRunnerMock = new Mock<ICodeRunner>();

            _sandboxFactory = (inner) => _sandboxedRunnerMock.Object;

            var runners = new List<ICodeRunner> { _csharpRunnerMock.Object, _pythonRunnerMock.Object };
            _factory = new CodeRunnerFactory(runners, _sandboxFactory);
        }

        [Fact]
        public void Create_ShouldReturnCodeRunner_WhenLanguageIsSupported()
        {
            // Act
            var runner = _factory.Create("csharp", sandbox: false);

            // Assert
            runner.Should().Be(_csharpRunnerMock.Object);
        }

        [Fact]
        public void Create_ShouldThrowUnsupportedLanguageException_WhenLanguageIsNotSupported()
        {
            // Act
            var act = () => _factory.Create("java");

            // Assert
            act.Should().Throw<UnsupportedLanguageException>();
        }

        [Fact]
        public void Create_ShouldReturnSandboxedRunner_WhenSandboxIsTrue()
        {
            // Act
            var runner = _factory.Create("csharp", sandbox: true);

            // Assert
            runner.Should().Be(_sandboxedRunnerMock.Object);
        }

        [Fact]
        public void Create_ShouldReturnUnsandboxedRunner_WhenSandboxIsFalse()
        {
            // Act
            var runner = _factory.Create("python", sandbox: false);

            // Assert
            runner.Should().Be(_pythonRunnerMock.Object);
        }

        [Fact]
        public void GetSupportedLanguages_ShouldReturnAllLanguages()
        {
            // Act
            var languages = _factory.GetSupportedLanguages();

            // Assert
            languages.Select(l => l.Name).Should().Contain(new[] { "csharp", "python" });
        }
    }
}
