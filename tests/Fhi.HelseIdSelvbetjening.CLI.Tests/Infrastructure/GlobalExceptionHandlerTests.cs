using Fhi.HelseIdSelvbetjening.CLI.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Text.Json;

namespace Fhi.HelseIdSelvbetjening.CLI.Tests.Infrastructure
{
    [TestFixture]
    public class GlobalExceptionHandlerTests
    {
        private ILogger<GlobalExceptionHandler> _logger;
        private GlobalExceptionHandler _exceptionHandler;

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
            _exceptionHandler = new GlobalExceptionHandler(_logger);
        }

        [Test]
        public async Task WrapHandler_WithAsyncHandler_LogsCorrelationId()
        {
            // Arrange
            var executed = false;
            Func<Task> handler = async () =>
            {
                executed = true;
                await Task.CompletedTask;
            };

            // Act
            var wrappedHandler = _exceptionHandler.WrapHandler(handler);
            await wrappedHandler();

            // Assert
            Assert.That(executed, Is.True);
            _logger.Received(1).LogDebug(Arg.Is<string>(s => s.Contains("Executing command handler")), Arg.Any<object[]>());
            _logger.Received(1).LogDebug(Arg.Is<string>(s => s.Contains("completed successfully")), Arg.Any<object[]>());
        }

        [Test]
        public async Task WrapHandler_WithException_HandlesGracefully()
        {
            // Arrange
            var testException = new ArgumentException("Test exception");
            Func<Task> handler = () => throw testException;

            // Act
            var wrappedHandler = _exceptionHandler.WrapHandler(handler);
            await wrappedHandler();

            // Assert
            _logger.Received(1).LogError(
                testException,
                Arg.Is<string>(s => s.Contains("unhandled exception")),
                Arg.Any<object[]>());
            
            Assert.That(Environment.ExitCode, Is.EqualTo(4)); // ArgumentException exit code
        }

        [Test]
        public async Task WrapHandler_WithParameterizedHandler_LogsParameters()
        {
            // Arrange
            var testParam = "test-parameter";
            var executed = false;
            Func<string, Task> handler = async (param) =>
            {
                executed = true;
                await Task.CompletedTask;
            };

            // Act
            var wrappedHandler = _exceptionHandler.WrapHandler(handler);
            await wrappedHandler(testParam);

            // Assert
            Assert.That(executed, Is.True);
            _logger.Received(1).LogDebug(
                Arg.Is<string>(s => s.Contains("Executing command handler with parameter")),
                Arg.Is<object[]>(args => args.Any(arg => arg.ToString() == testParam)));
        }

        [Test]
        public void WrapHandler_WithSynchronousHandler_ExecutesCorrectly()
        {
            // Arrange
            var executed = false;
            Action handler = () => executed = true;

            // Act
            var wrappedHandler = _exceptionHandler.WrapHandler(handler);
            wrappedHandler();

            // Assert
            Assert.That(executed, Is.True);
            _logger.Received(1).LogDebug(Arg.Is<string>(s => s.Contains("Executing synchronous command handler")), Arg.Any<object[]>());
        }

        [Test]
        public async Task WrapHandler_WithJsonException_ReturnsCorrectExitCode()
        {
            // Arrange
            var jsonException = new JsonException("Invalid JSON format");
            Func<Task> handler = () => throw jsonException;

            // Act
            var wrappedHandler = _exceptionHandler.WrapHandler(handler);
            await wrappedHandler();

            // Assert
            Assert.That(Environment.ExitCode, Is.EqualTo(8)); // JsonException exit code
        }

        [Test]
        public async Task WrapHandler_WithFileNotFoundException_ReturnsCorrectExitCode()
        {
            // Arrange
            var fileException = new FileNotFoundException("File not found", "test.txt");
            Func<Task> handler = () => throw fileException;

            // Act
            var wrappedHandler = _exceptionHandler.WrapHandler(handler);
            await wrappedHandler();

            // Assert
            Assert.That(Environment.ExitCode, Is.EqualTo(2)); // FileNotFoundException exit code
        }

        [Test]
        public async Task WrapHandler_WithInnerException_LogsInnerExceptionDetails()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner exception");
            var outerException = new Exception("Outer exception", innerException);
            Func<Task> handler = () => throw outerException;

            // Act
            var wrappedHandler = _exceptionHandler.WrapHandler(handler);
            await wrappedHandler();

            // Assert
            _logger.Received(1).LogError(
                innerException,
                Arg.Is<string>(s => s.Contains("Inner exception details")),
                Arg.Any<object[]>());
        }

        [TearDown]
        public void TearDown()
        {
            // Reset exit code for next test
            Environment.ExitCode = 0;
        }
    }
}
