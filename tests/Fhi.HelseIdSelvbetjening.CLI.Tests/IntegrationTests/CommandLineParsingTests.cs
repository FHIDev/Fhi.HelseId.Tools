using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
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
    /// Tests for command line parsing scenarios in ClientSecretExpirationReader.
    /// These tests verify that complex JSON strings with special characters,
    /// quotes, and other command-line parsing challenges are handled correctly.
    /// </summary>
    [TestFixture]
    public class CommandLineParsingTests
    {
        private ILogger<ClientSecretExpirationReaderService> _loggerMock = null!;
        private IHelseIdSelvbetjeningService _helseIdServiceMock = null!;
        private FileHandlerMock _fileHandlerMock = null!;
        private const string TestClientId = "test-client-id";

        [SetUp]
        public void SetUp()
        {
            _loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            _helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            _fileHandlerMock = new FileHandlerMock();

            // Setup default successful response
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.UtcNow.AddDays(30)
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
        }

        [TestCase(@"{""test"":""json"",""key"":""value""}", "JSON with quotes")]
        [TestCase(@"{""kid"":""test-with-special-chars-!@#$%^&*()"",""d"":""data-with-quotes-\""and\""-backslashes-\\"",""n"":""modulus"",""e"":""AQAB""}", "JSON with special characters and escaped quotes")]
        [TestCase(@"{""test"":""json""}", "simple JSON that could cause command line parsing issues")]
        public async Task ReadClientSecretExpiration_WithCommandLineParsingChallenges_ShouldHandleCorrectly(string problematicJson, string description)
        {
            // Arrange
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, problematicJson);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), $"Command should complete successfully with {description}");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == TestClientId && c.Jwk == problematicJson)
                );
                _loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication with Client ID") && o.ToString()!.Contains("Key ID (kid)")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithFilePathAndDirectJwk_ShouldPrioritizeDirectJwk()
        {
            // Arrange
            const string directJwk = @"{""kid"":""direct-jwk"",""kty"":""RSA""}";
            const string filePath = @"c:\temp\jwk.json";
            const string fileJwk = @"{""kid"":""file-jwk"",""kty"":""RSA""}";
            _fileHandlerMock.Files[filePath] = fileJwk;

            var args = CLITestUtilities.CreateBothJwkArgs(TestClientId, directJwk, filePath);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == TestClientId && c.Jwk == directJwk)
                );
                await _helseIdServiceMock.DidNotReceive().ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.Jwk == fileJwk)
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_Success_LogsEpochTimestamp()
        {
            // Arrange
            var mockResponse = new ClientSecretExpirationResponse(HttpStatusCode.OK, null, DateTime.UtcNow.AddDays(30));
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);

            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, @"{""kty"":""RSA""}");
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            await rootCommand.InvokeAsync(args);

            // Assert - Should log a valid epoch timestamp
            _loggerMock.Received().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => CLITestUtilities.IsValidEpochTimestamp(o.ToString()!)),
                null,
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Test]
        public async Task ReadClientSecretExpiration_FailedRequest_ShouldLogErrorAndSetExitCode1()
        {
            // Arrange
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.BadRequest,
                "Invalid client",
                DateTime.MinValue
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);

            var testJwk = @"{""kid"":""test-key"",""kty"":""RSA""}";
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, testJwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command invocation should complete (Environment.ExitCode is set separately)");
                Assert.That(Environment.ExitCode, Is.EqualTo(1), "Exit code should be set to 1");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(Arg.Any<ClientConfiguration>());
                _loggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Failed to read expiration")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_NoExpirationDate_ShouldLogErrorAndSetExitCode1()
        {
            // Arrange
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                "Success but no expiration",
                DateTime.MinValue
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);

            var testJwk = @"{""kid"":""test-key"",""kty"":""RSA""}";
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, testJwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command invocation should complete (Environment.ExitCode is set separately)");
                Assert.That(Environment.ExitCode, Is.EqualTo(1), "Exit code should be set to 1");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(Arg.Any<ClientConfiguration>());
                _loggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Expiration date not found")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithMissingPrivateKey_ShouldLogErrorAndSetExitCode1()
        {
            // Arrange - no private key provided
            var args = new[] { "readclientsecretexpiration", "-c", TestClientId };
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command invocation should complete (Environment.ExitCode is set separately)");
                Assert.That(Environment.ExitCode, Is.EqualTo(1), "Exit code should be set to 1");
                await _helseIdServiceMock.DidNotReceive().ReadClientSecretExpiration(Arg.Any<ClientConfiguration>());
                _loggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("No private key provided")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

    }
}
