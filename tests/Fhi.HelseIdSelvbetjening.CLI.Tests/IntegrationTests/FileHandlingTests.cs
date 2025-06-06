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
    /// Tests for file handling scenarios in ClientSecretExpirationReader.
    /// These tests verify that JWK files are correctly read and processed,
    /// including complex JSON content that might cause command-line parsing issues.
    /// </summary>
    [TestFixture]
    public class FileHandlingTests
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
                DateTime.Now.AddDays(30)
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
        }

        [Test]
        [Category("RealWorldScenarios")]
        public async Task ReadClientSecretExpiration_WithFilePathAsWorkaround_ShouldAvoidCommandLineParsingIssues()
        {
            // Arrange
            const string filePath = @"c:\temp\complex-jwk.json";
            const string complexJson = """
                {
                    "kid": "test-kid-with-special-chars-!@#$%^&*()",
                    "kty": "RSA",
                    "d": "test-d-value-with-quotes-\"and\"-backslashes-\\",
                    "n": "test-n-value",
                    "e": "AQAB",
                    "custom": "This contains \"nested quotes\" and 'single quotes' and json keyword"
                }
                """;
            _fileHandlerMock.Files[filePath] = complexJson;

            var args = CLITestUtilities.CreateFilePathArgs(TestClientId, filePath);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully when using file path");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == TestClientId && c.Jwk == complexJson)
                );
                _loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }
    }
}
