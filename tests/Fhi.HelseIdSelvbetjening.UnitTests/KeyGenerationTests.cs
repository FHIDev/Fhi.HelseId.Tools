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
            var parameters = new GenerateKeyParameters { KeyFileNamePrefix = "TestClient", KeyDirectory = "C:\\TestKeys" };
            var fileStore = new FileHandlerMock();

            var service = new KeyGeneratorService(parameters, fileStore, loggerMock);
            await service.StartAsync(CancellationToken.None);

            var expectedPublicKeyPath = Path.Combine(parameters.KeyDirectory, $"{parameters.KeyFileNamePrefix}_public.json");
            var expectedPrivateKeyPath = Path.Combine(parameters.KeyDirectory, $"{parameters.KeyFileNamePrefix}_private.json");
            loggerMock.Received(1).Log(
               LogLevel.Information,
               Arg.Any<EventId>(),
               Arg.Is<object>(o => o.ToString()!.Contains($"Private key saved: {expectedPrivateKeyPath}")),
               Arg.Any<Exception>(),
               Arg.Any<Func<object, Exception?, string>>());

            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains($"Public key saved: {expectedPublicKeyPath}")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());


            var privateKey = fileStore._files[expectedPrivateKeyPath];
            var privateJwk = new JsonWebKey(privateKey);
            Assert.That(privateJwk, Is.Not.Null);
            Assert.That(privateJwk.Alg, Is.EqualTo(SecurityAlgorithms.RsaSha512));
        }

        [Test]
        public async Task GenerateKeys_PathIsEmpty_UseCurrentDirectory()
        {
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            var parameters = new GenerateKeyParameters { KeyFileNamePrefix = "TestClient" };
            var fileStore = new FileHandlerMock();
            using var output = new StringWriter();
            Console.SetOut(output);

            var service = new KeyGeneratorService(parameters, fileStore, loggerMock);

            await service.StartAsync(CancellationToken.None);

            var expectedPublicKeyPath = Path.Combine(Environment.CurrentDirectory, $"{parameters.KeyFileNamePrefix}_public.json");
            var expectedPrivateKeyPath = Path.Combine(Environment.CurrentDirectory, $"{parameters.KeyFileNamePrefix}_private.json");

            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains($"Private key saved: {expectedPrivateKeyPath}")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());

            loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains($"Public key saved: {expectedPublicKeyPath}")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());

            var privateKey = fileStore._files[expectedPrivateKeyPath];
            var privateJwk = new JsonWebKey(privateKey);
            Assert.That(privateJwk, Is.Not.Null);
            Assert.That(privateJwk.Alg, Is.EqualTo(SecurityAlgorithms.RsaSha512));
        }
    }
}
