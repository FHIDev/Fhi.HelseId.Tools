using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    public class ClientKeyUpdateTests
    {
        [TestCase("", "c:\\temp")]
        [TestCase("c:\\temp", "")]
        public async Task ClientKeyUpdate_EmptyNewKeyPath_GiveErrorMessage(string newKeyPath, string oldkeyPath)
        {
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var parameters = new UpdateClientKeyParameters { ClientId = "TestClient", NewPublicJwkPath = newKeyPath, ExistingPrivateJwkPath = oldkeyPath };
            var fileHandlerMock = new FileHandlerMock();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var clientKeyUpdaterService = new ClientKeyUpdaterService(parameters, helseIdServiceMock, fileHandlerMock, loggerMock);

            await clientKeyUpdaterService.ExecuteAsync();

            loggerMock.Received(1).Log(
               LogLevel.Error,
               Arg.Any<EventId>(),
               Arg.Is<object>(o => o.ToString()!.Contains("Parameters empty.")),
               Arg.Any<Exception>(),
               Arg.Any<Func<object, Exception?, string>>());
        }

        [Test]
        public async Task ClientKeyUpdate_WithValidParameters_UpdatesClientSecret()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var fileHandlerMock = new FileHandlerMock();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            
            // Setup test data
            var clientId = "test-client-id";
            var existingPrivateJwkPath = "c:\\temp\\existing-private.json";
            var newPublicJwkPath = "c:\\temp\\new-public.json";
            var existingPrivateJwk = "{\"kid\":\"test-kid\",\"kty\":\"RSA\",\"private\":\"key-data\"}";
            var newPublicJwk = "{\"kid\":\"new-kid\",\"kty\":\"RSA\",\"public\":\"key-data\"}";
            
            // Setup file handler mock to return the key contents
            fileHandlerMock._files[existingPrivateJwkPath] = existingPrivateJwk;
            fileHandlerMock._files[newPublicJwkPath] = newPublicJwk;
            
            var parameters = new UpdateClientKeyParameters 
            { 
                ClientId = clientId, 
                NewPublicJwkPath = newPublicJwkPath, 
                ExistingPrivateJwkPath = existingPrivateJwkPath 
            };
            
            var clientKeyUpdaterService = new ClientKeyUpdaterService(
                parameters, 
                helseIdServiceMock, 
                fileHandlerMock, 
                loggerMock
            );

            // Act
            await clientKeyUpdaterService.ExecuteAsync();

            // Assert
            // Verify the HelseId service was called with correct parameters
            await helseIdServiceMock.Received(1).UpdateClientSecret(
                Arg.Is<ClientConfiguration>(c => c.ClientId == clientId),
                Arg.Is<string>(s => s == newPublicJwk)
            );
            
            // Verify proper logging messages were logged
            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains($"Update client {clientId}")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
            
            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("NewKey:")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
            
            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("OldKey:")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }
    }
}
