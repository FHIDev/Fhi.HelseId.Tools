using Fhi.HelseIdSelvbetjening.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    public class KeyGenerationTests
    {
        [Test]
        public async Task GenerateKeys_PathIsNotEmpty_AddKeysToSpecifiedPath()
        {
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            var parameters = new GenerateKeyParameters { FileName = "TestClient", KeyPath = "C:\\TestKeys" };
            var fileStore = new FileHandlerMock();

            var service = new KeyGeneratorService(parameters, fileStore, loggerMock);
            await service.StartAsync(CancellationToken.None);

            loggerMock.Received(1).Log(
               LogLevel.Information,
               Arg.Any<EventId>(),
               Arg.Is<object>(o => o.ToString()!.Contains("Private key saved: C:\\TestKeys\\TestClient_private.json")),
               Arg.Any<Exception>(),
               Arg.Any<Func<object, Exception?, string>>());

            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Public key saved: C:\\TestKeys\\TestClient_public.json")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());


            var privateKey = fileStore._files["C:\\TestKeys\\TestClient_private.json"];
            var privateJwk = new JsonWebKey(privateKey);
            Assert.That(privateJwk, Is.Not.Null);
            Assert.That(privateJwk.Alg, Is.EqualTo(SecurityAlgorithms.RsaSha512));
        }

        [Test]
        public async Task GenerateKeys_PathIsEmpty_UseCurrentDirectory()
        {
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            var parameters = new GenerateKeyParameters { FileName = "TestClient" };
            var fileStore = new FileHandlerMock();
            using var output = new StringWriter();
            Console.SetOut(output);

            var service = new KeyGeneratorService(parameters, fileStore, loggerMock);

            await service.StartAsync(CancellationToken.None);

            Console.WriteLine($"Current directory: {Environment.CurrentDirectory}");
            var currentDir = Environment.CurrentDirectory;
            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains($"Private key saved: {currentDir}\\TestClient_private.json")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());

            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains($"Public key saved: {currentDir}\\TestClient_public.json")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());

            var privateKey = fileStore._files[$"{currentDir}\\TestClient_private.json"];
            var privateJwk = new JsonWebKey(privateKey);
            Assert.That(privateJwk, Is.Not.Null);
            Assert.That(privateJwk.Alg, Is.EqualTo(SecurityAlgorithms.RsaSha512));
        }
    }
}
