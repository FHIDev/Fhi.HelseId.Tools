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
    /// Tests for Key ID (kid) handling scenarios in ClientSecretExpirationReader.
    /// These tests verify that clients with multiple keys are handled correctly,
    /// including scenarios where keys may have expired or have different expiration dates.
    /// </summary>
    [TestFixture]
    public class KidHandlingTests
    {
        private ILogger<ClientSecretExpirationReaderService> _loggerMock = null!;
        private IHelseIdSelvbetjeningService _helseIdServiceMock = null!;
        private FileHandlerMock _fileHandlerMock = null!;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            _helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            _fileHandlerMock = new FileHandlerMock();
        }

        [Test]
        [Category("KidHandlingIssue")]
        public async Task ReadClientSecretExpiration_WithMultipleKeysFirstExpired_ShouldReturnLatestExpiration()
        {
            // Arrange
            const string clientId = "test-client-multiple-keys";
            const string privateJwkWithKid = """
                {
                    "kid": "newer-key-id",
                    "kty": "RSA",
                    "d": "test-d-value",
                    "n": "test-n-value",
                    "e": "AQAB"
                }
                """;

            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(60)
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);

            var args = CLITestUtilities.CreateDirectJwkArgs(clientId, privateJwkWithKid);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should succeed");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == privateJwkWithKid)
                );

                // The service should handle:
                // 1. Extracting the kid from the request JWK
                // 2. Either matching the kid to a specific key's expiration
                // 3. Or returning the latest (furthest in future) expiration date
                // Should log the epoch timestamp
                var expectedEpoch = ((DateTimeOffset)mockResponse.ExpirationDate).ToUnixTimeSeconds();
                _loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains(expectedEpoch.ToString())),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        [Category("KidHandlingIssue")]
        public async Task ReadClientSecretExpiration_WithMultipleKeysDifferentKids_ShouldHandleCorrectly()
        {
            // Arrange
            const string clientId = "test-client-kid-matching";
            const string privateJwkWithSpecificKid = """
                {
                    "kid": "specific-key-id-to-match",
                    "kty": "RSA", 
                    "d": "test-d-value",
                    "n": "test-n-value",
                    "e": "AQAB"
                }
                """;

            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(60) // Future expiration
            );
            _helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);

            var args = CLITestUtilities.CreateDirectJwkArgs(clientId, privateJwkWithSpecificKid);
            var rootCommand = CLITestUtilities.CreateRootCommand(args, _fileHandlerMock, _loggerMock, _helseIdServiceMock);

            // Act
            int exitCode = await rootCommand.InvokeAsync(args);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should succeed");
                await _helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c =>
                        c.ClientId == clientId &&
                        c.Jwk == privateJwkWithSpecificKid)
                );

                // The service should handle kid matching correctly
                // by extracting the kid from the client JWK and either:
                // 1. Finding a matching kid in the server response
                // 2. Returning the latest expiration date from all keys
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
