using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.Net;
using System.Text;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    /// <summary>
    /// Integration tests that capture actual console output to verify the end-to-end behavior
    /// that the user (colleague) will experience when using the tool in Octopus.
    /// </summary>
    [TestFixture]
    public class ConsoleOutputTests
    {
        private ILogger<ClientSecretExpirationReaderService> _loggerMock = null!;
        private IHelseIdSelvbetjeningService _helseIdServiceMock = null!;
        private FileHandlerMock _fileHandlerMock = null!;
        private const string TestClientId = "test-client-for-octopus";
        private StringWriter _consoleOutput = null!;
        private TextWriter _originalConsoleOut = null!;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            _helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            _fileHandlerMock = new FileHandlerMock();

            // Capture console output
            _originalConsoleOut = Console.Out;
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);

            // Reset Environment.ExitCode for each test
            Environment.ExitCode = 0;
        }

        [TearDown]
        public void TearDown()
        {
            Console.SetOut(_originalConsoleOut);
            _consoleOutput?.Dispose();
            Environment.ExitCode = 0;
        }


        [Test]
        public async Task ReadClientSecretExpiration_FailureScenario_ShouldLogErrorAndReturnNonZeroExitCode()
        {
            // Arrange - Simulate failure scenario
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.BadRequest,
                "Invalid client credentials",
                DateTime.MinValue
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);

            var testJwk = @"{""kid"":""invalid-key"",""kty"":""RSA"",""d"":""invalid"",""n"":""invalid"",""e"":""AQAB""}";
            var args = CLITestUtilities.CreateDirectJwkArgs(TestClientId, testJwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command invocation completes but sets Environment.ExitCode");
                Assert.That(Environment.ExitCode, Is.EqualTo(1), "Environment exit code should be 1 for failure");

                // Should log error message
                _loggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Failed to read expiration")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );

                // Should NOT log any epoch timestamp (no success)
                _loggerMock.DidNotReceive().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => CLITestUtilities.IsValidEpochTimestamp(o.ToString()!)),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_NoPrivateKey_ShouldLogErrorWithoutServiceCall()
        {
            // Arrange - Missing private key scenario
            var args = new[] { "readclientsecretexpiration", "-c", TestClientId };
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command invocation completes");
                Assert.That(Environment.ExitCode, Is.EqualTo(1), "Environment exit code should be 1");

                // Should never call the service
                await _helseIdServiceMock.DidNotReceive().ReadClientSecretExpiration(Arg.Any<ClientConfiguration>());

                // Should log error about missing private key
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
        public async Task ReadClientSecretExpiration_OctopusRunbookScenario_SimulateRealUsage()
        {
            // Arrange - This simulates exactly what your colleague's Octopus runbook will do
            var futureExpirationDate = DateTime.UtcNow.AddDays(45); // Key expires in 45 days
            var expectedEpoch = ((DateTimeOffset)futureExpirationDate).ToUnixTimeSeconds();
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                "Success",
                futureExpirationDate
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);

            // Simulate real JWK that Octopus would use
            var realWorldJwk = @"{
                ""kid"": ""octopus-production-key-2025"",
                ""kty"": ""RSA"",
                ""d"": ""MIIEpAIBAAKCAQEA7S5J..."",
                ""n"": ""7S5J1w5z2....."",
                ""e"": ""AQAB"",
                ""alg"": ""RS256"",
                ""use"": ""sig""
            }";

            var args = CLITestUtilities.CreateDirectJwkArgs("production-client-id", realWorldJwk);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act - What Octopus will execute
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert - What Octopus will see
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Octopus should see successful command execution");
                Assert.That(Environment.ExitCode, Is.EqualTo(0), "Process should exit with code 0");

                // Verify the logger was called with the epoch timestamp (which Serilog will output to console)
                _loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString() == expectedEpoch.ToString()),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );

                // Verify proper service call
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == "production-client-id" && c.Jwk == realWorldJwk)
                );
            }
        }
    }
}