using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.Net;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    /// <summary>
    /// Tests for validation error scenarios in ClientSecretExpirationReader.
    /// These tests verify that proper validation errors are logged and displayed
    /// when invalid input is provided.
    /// </summary>
    [TestFixture]
    public class ValidationErrorTests
    {
        private ILogger<Commands.ReadClientSecretExpiration.ClientSecretExpirationReaderService> _loggerMock = null!;
        private IHelseIdSelvbetjeningService _helseIdServiceMock = null!;
        private FileHandlerMock _fileHandlerMock = null!;
        private const string TestClientId = "test-client-id";

        [SetUp]
        public void SetUp()
        {
            _loggerMock = Substitute.For<ILogger<Commands.ReadClientSecretExpiration.ClientSecretExpirationReaderService>>();
            _helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            _fileHandlerMock = new FileHandlerMock();
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithEmptyExistingPrivateJwk_ShouldFail()
        {
            // Arrange
            const string emptyJwk = "";
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, emptyJwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            _loggerMock.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("No private key provided")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithWhitespaceOnlyJwk_ShouldFail()
        {
            // Arrange
            const string whitespaceJwk = "   \n\t  \r\n  ";
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, whitespaceJwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            _loggerMock.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("No private key provided")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithEmptyClientId_ShouldLogValidationError()
        {
            // Arrange
            const string validJwk = "{\"kty\":\"RSA\",\"d\":\"test\",\"n\":\"test\",\"e\":\"AQAB\"}";
            
            // Mock validation error response for empty ClientId
            var validationErrorResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.BadRequest,
                "Validation failed",
                DateTime.MinValue,
                new List<string> { "ClientId cannot be null or empty" }
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(validationErrorResponse);

            var args = CLITestUtilities.CreateDirectJwkArgs("", validJwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == "" && c.Jwk == validJwk)
                );
                _loggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Validation failed")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithEmptyJwk_ShouldLogValidationError()
        {
            // Arrange
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, "");
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                // The CLI should not call the service when JWK is empty
                await _helseIdServiceMock.DidNotReceive().ReadClientSecretExpiration(Arg.Any<ClientConfiguration>());

                // Instead, it should log about no private key being provided
                _loggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("No private key provided")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithServiceValidationErrors_ShouldDisplayValidationErrors()
        {
            // Arrange
            const string clientId = "";
            const string validJwk = @"{""kid"":""test-kid"",""kty"":""RSA"",""d"":""test-d"",""n"":""test-n"",""e"":""AQAB""}";
            
            // Mock the service to return validation errors
            var validationErrors = new List<string> { "ClientId cannot be null or empty", "Jwk cannot be null or empty" };
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.BadRequest,
                "Validation failed",
                DateTime.MinValue,
                validationErrors
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);

            var args = CLITestUtilities.CreateDirectJwkArgs(clientId, validJwk);

            // Capture console output
            using var stringWriter = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);
                
                // Act
                int exitCode = await rootCommand.InvokeAsync(args);

                var consoleOutput = stringWriter.ToString();
                
                // Assert
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(exitCode, Is.EqualTo(0), "Command should complete even with validation errors");
                    Assert.That(consoleOutput, Does.Contain("Validation errors:"), "Should display validation errors header");
                    Assert.That(consoleOutput, Does.Contain("- ClientId cannot be null or empty"), "Should display specific validation error");
                    Assert.That(consoleOutput, Does.Contain("- Jwk cannot be null or empty"), "Should display specific validation error");

                    // Verify logging
                    _loggerMock.Received().Log(
                        LogLevel.Error,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o => o.ToString()!.Contains("Validation failed")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?, string>>()
                    );
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
