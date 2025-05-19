using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fhi.HelseIdSelvbetjening.CLI.UnitTests
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
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();

            var clientId = "test-client-id";
            var existingPrivateJwkPath = "c:\\temp\\existing-private.json";
            var newPublicJwkPath = "c:\\temp\\new-public.json";
            var existingPrivateJwk = "{\"kid\":\"test-kid\",\"kty\":\"RSA\",\"private\":\"key-data\"}";
            var newPublicJwk = "{\"kid\":\"new-kid\",\"kty\":\"RSA\",\"public\":\"key-data\"}";

            var fileHandlerMock = new FileHandlerMock();
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

            await clientKeyUpdaterService.ExecuteAsync();

            await helseIdServiceMock.Received(1).UpdateClientSecret(
                Arg.Is<ClientConfiguration>(c => c.ClientId == clientId),
                Arg.Is<string>(s => s == newPublicJwk)
            );

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
