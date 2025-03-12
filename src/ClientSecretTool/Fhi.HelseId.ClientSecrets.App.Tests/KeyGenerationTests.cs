using Fhi.HelseId.ClientSecret.App.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static Program;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    public class KeyGenerationTests
    {
        [Test]
        public async Task GenerateKeys_PathIsEmpty_UseCurrentDirectory()
        {
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            var parameters = new GenerateKeyParameters { FileName = "TestClient", KeyPath = "C:\\TestKeys" };

            using var output = new StringWriter();

            var service = new KeyGeneratorService(parameters, loggerMock);

            await service.StartAsync(CancellationToken.None);

            string consoleOutput = output.ToString();
        }

    }
}
