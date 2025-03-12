using Fhi.HelseId.Selvbetjening;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static Program;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    public class ClientKeyUpdateTests
    {
        [Test]
        public async Task ClientKeyUpate_UserConfirms_UpdateClientSecret()
        {
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var parameters = new UpdateClientKeyParameters { ClientId = "TestClient", NewKeyPath = "" };

            using var input = new StringReader("y\n"); // Simulate user typing 'y'
            using var output = new StringWriter();
            Console.SetIn(input);
            Console.SetOut(output);

            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var service = new ClientKeyUpdaterService(parameters, helseIdServiceMock, loggerMock);

            await service.StartAsync(CancellationToken.None);

            string consoleOutput = output.ToString();
        }

    }
}
