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
    /// Tests for JSON format handling in ClientSecretExpirationReader.
    /// These tests verify that various JSON formats are correctly parsed and processed,
    /// including edge cases with newlines, special characters, and different formatting styles.
    /// </summary>
    [TestFixture]
    public class JsonFormatHandlingTests
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

        [TestCase("{\n  \"kid\": \"test-kid\",\n  \"kty\": \"RSA\",\n  \"d\": \"test-d-value\",\n  \"n\": \"test-n-value\",\n  \"e\": \"AQAB\"\n}", "pretty formatted JSON with newlines")]
        [TestCase("{\"d\":\"test-d-value\",\"e\":\"AQAB\",\"kid\":\"test-kid\",\"kty\":\"RSA\",\"n\":\"test-n-value\"}", "compact JSON without newlines")]
        [TestCase("{ \"kid\": \"test-kid\", \"kty\": \"RSA\", \"d\": \"test-data\", \"n\": \"test-modulus\", \"e\": \"AQAB\" }", "JSON with spaces")]
        public async Task ReadClientSecretExpiration_WithVariousJsonFormats_ShouldSucceed(string jwk, string description)
        {
            // Arrange
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, jwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), $"Command should complete successfully with {description}");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == TestClientId && c.Jwk == jwk)
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
        public async Task ReadClientSecretExpiration_WithRealWorldJwkFromFile_ShouldSucceed()
        {
            // Arrange
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, CLITestUtilities.TestJwkSamples.RealWorldJwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully with real-world JWK");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == TestClientId && c.Jwk == CLITestUtilities.TestJwkSamples.RealWorldJwk)
                );
                // Should log a valid epoch timestamp
                _loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => CLITestUtilities.IsValidEpochTimestamp(o.ToString()!)),
                    null,
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithJsonContainingJsonKeyword_ShouldSucceed()
        {
            // Arrange
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, CLITestUtilities.TestJwkSamples.ComplexJwkWithSpecialChars);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully with JSON containing 'json' keyword");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == TestClientId && c.Jwk == CLITestUtilities.TestJwkSamples.ComplexJwkWithSpecialChars)
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
        public async Task ReadClientSecretExpiration_WithMalformedButValidStringJson_ShouldHandleGracefully()
        {
            // Arrange
            const string potentiallyProblematicJson = """
                {
                    "kty": "RSA",
                    "kid": "test-kid-with-json-in-name",
                    "d": "invalid-but-string-value-that-might-cause-issues",
                    "n": "another-invalid-but-string-value",
                    "e": "not-standard-AQAB",
                    "json": "json",
                    "description": "json with json everywhere"
                }
                """;

            // Return a validation error response instead of throwing an exception
            var validationErrorResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.BadRequest,
                "Invalid JWK format",
                DateTime.MinValue,
                new List<string> { "Invalid JWK format" }
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(validationErrorResponse);

            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, potentiallyProblematicJson);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                Arg.Is<ClientConfiguration>(c => c.ClientId == TestClientId && c.Jwk == potentiallyProblematicJson)
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
}
