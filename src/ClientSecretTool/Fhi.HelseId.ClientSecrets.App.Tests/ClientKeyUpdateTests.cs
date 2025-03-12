using Microsoft.Extensions.Logging;
using NSubstitute;
using static Program;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    public class ClientKeyUpdateTests
    {
        [Test]
        public void ClientKeyUpate_UserConfirms_UpdateClientSecret()
        {
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var parameters = new GenerateKeyParameters { ClientId = "TestClient", KeyPath = "C:\\TestKeys" };

            using var input = new StringReader("y\n"); // Simulate user typing 'y'
            using var output = new StringWriter();
            Console.SetIn(input);
            Console.SetOut(output);

            //var service = new ClientKeyUpdaterService(parameters, loggerMock);

            //await service.StartAsync(CancellationToken.None);

            //string consoleOutput = output.ToString();
        }

    }
}
