using Fhi.HelseIdSelvbetjening.Services;
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
    }
}
