using Fhi.HelseId.ClientSecret.App.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static Program;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    public class KeyGenerationTests
    {
        [Test]
        public async Task GenerateKeys_UserConfirms()
        {
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            var parameters = new GenerateKeyParameters { ClientId = "TestClient", KeyPath = "C:\\TestKeys" };

            using var input = new StringReader("y\n"); // Simulate user typing 'y'
            using var output = new StringWriter();
            Console.SetIn(input);
            Console.SetOut(output);

            var service = new KeyGeneratorService(parameters, loggerMock);

            await service.StartAsync(CancellationToken.None);

            string consoleOutput = output.ToString();
        }

    }
}
