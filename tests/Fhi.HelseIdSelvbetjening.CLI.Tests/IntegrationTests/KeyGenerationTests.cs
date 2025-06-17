using Fhi.HelseIdSelvbetjening.CLI.Commands;
using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using System.CommandLine;
namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public class KeyGenerationTests
    {
        [TestCase(GenerateKeyParameterNames.KeyFileNamePrefixLong, GenerateKeyParameterNames.KeyDirectoryLong)]
        [TestCase(GenerateKeyParameterNames.KeyFileNamePrefixShort, GenerateKeyParameterNames.KeyDirectoryShort)]
        public async Task GenerateKeys(string filePrefix, string directory)
        {
            var fileHandlerMock = new FileHandlerMock();
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            var args = new[]
            {
                GenerateKeyParameterNames.CommandName,
                $"--{filePrefix}", "integration_test",
                $"--{directory}", "c:\\temp"
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(fileHandlerMock.Files, Has.Count.EqualTo(2));
                Assert.That(exitCode, Is.EqualTo(0));
            }
            loggerMock.Received(1).Log(
              LogLevel.Information,
              Arg.Any<EventId>(),
              Arg.Is<object>(o => o.ToString()!.Contains($"Private key saved:")),
              Arg.Any<Exception>(),
              Arg.Any<Func<object, Exception?, string>>());
            loggerMock.Received(1).Log(
              LogLevel.Information,
              Arg.Any<EventId>(),
              Arg.Is<object>(o => o.ToString()!.Contains($"Public key saved:")),
              Arg.Any<Exception>(),
              Arg.Any<Func<object, Exception?, string>>());
        }
        [Test]
        [Ignore("TODO: figure out how to return proper message")]
        public async Task GenerateKeys_InvalidParameterAsync()
        {
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var args = new[]
            {
                GenerateKeyParameterNames.CommandName,
                "--invalidparameter", "integration_test"
            };
            var rootCommand = CreateRootCommand(args, new FileHandlerMock(), loggerMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            var output = stringWriter.ToString();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Not.EqualTo(0));
                //TODO:Figure out what error message should be
                Assert.That(output, Does.Contain("Unrecognized option '--invalidparameter'").IgnoreCase
                    .Or.Contain("Unknown option").IgnoreCase
                    .Or.Contain("is not a recognized option").IgnoreCase);
            }
            loggerMock.DidNotReceive().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Private key saved:")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
        [Test]
        public async Task GenerateKeys_PathIsNotEmpty_AddKeysToSpecifiedPath()
        {
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            var fileHandlerMock = new FileHandlerMock();
            var args = new[]
            {
                GenerateKeyParameterNames.CommandName,
                $"--{GenerateKeyParameterNames.KeyFileNamePrefixLong}", "TestClient",
                $"--{GenerateKeyParameterNames.KeyDirectoryLong}", "C:\\TestKeys"
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            var expectedPublicKeyPath = Path.Combine("C:\\TestKeys", "TestClient_public.json");
            var expectedPrivateKeyPath = Path.Combine("C:\\TestKeys", "TestClient_private.json");
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
            var privateKey = fileHandlerMock.Files[expectedPrivateKeyPath];
            var privateJwk = new JsonWebKey(privateKey);
            Assert.That(privateJwk, Is.Not.Null);
            Assert.That(privateJwk.Alg, Is.EqualTo(SecurityAlgorithms.RsaSha512));
        }
        [Test]
        public async Task GenerateKeys_PathIsEmpty_UseCurrentDirectory()
        {
            var loggerMock = Substitute.For<ILogger<KeyGeneratorService>>();
            var fileHandlerMock = new FileHandlerMock();
            var args = new[]
            {
                GenerateKeyParameterNames.CommandName,
                $"--{GenerateKeyParameterNames.KeyFileNamePrefixLong}", "TestClient"
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            var expectedPublicKeyPath = Path.Combine(Environment.CurrentDirectory, "TestClient_public.json");
            var expectedPrivateKeyPath = Path.Combine(Environment.CurrentDirectory, "TestClient_private.json");
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
            var privateKey = fileHandlerMock.Files[expectedPrivateKeyPath];
            var privateJwk = new JsonWebKey(privateKey);
            Assert.That(privateJwk, Is.Not.Null);
            Assert.That(privateJwk.Alg, Is.EqualTo(SecurityAlgorithms.RsaSha512));
        }
        private static RootCommand CreateRootCommand(string[] args, FileHandlerMock fileHandlerMock, ILogger<KeyGeneratorService> loggerMock)
        {
            return Program.BuildRootCommand(new CommandInput()
            {
                Args = args,
                OverrideServices = services =>
                {
                    services.AddSingleton<IFileHandler>(fileHandlerMock);
                    services.AddSingleton(loggerMock);
                }
            });
        }
    }
}
