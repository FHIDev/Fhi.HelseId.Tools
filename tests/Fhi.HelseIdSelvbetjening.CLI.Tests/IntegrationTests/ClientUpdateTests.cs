using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using static Program;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public class ClientUpdateTests
    {
        [Test]
        public void UpdateClientKeysFromPath()
        {
            // TODO: Implement test logic for updating client keys from file path
        }

        [Test]
        public void UpdateClientKeysFromParameters()
        {
            // TODO: Implement test logic for updating client keys from parameters
        }

        [TestCase("", "c:\\temp")]
        [TestCase("c:\\temp", "")]
        public async Task ClientKeyUpdate_EmptyNewKeyPath_GiveErrorMessage(string newKeyPath, string oldkeyPath)
        {
            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName, "--env", "dev",
                $"--{UpdateClientKeyParameterNames.NewPublicJwk.Long}", "",
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwk.Long}", "",
                $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", newKeyPath,
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", oldkeyPath,
                $"--{UpdateClientKeyParameterNames.ClientId.Long}", "88d474a8-07df-4dc4-abb0-6b759c2b99ec"
            };
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var fileHandlerMock = new FileHandlerMock();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();

            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);

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

            const string clientId = "test-client-id";
            const string existingPrivateJwkPath = "c:\\temp\\existing-private.json";
            const string newPublicJwkPath = "c:\\temp\\new-public.json";
            const string existingPrivateJwk = "{\"kid\":\"test-kid\",\"kty\":\"RSA\",\"private\":\"key-data\"}";
            const string newPublicJwk = "{\"kid\":\"new-kid\",\"kty\":\"RSA\",\"public\":\"key-data\"}";

            var fileHandlerMock = new FileHandlerMock();
            fileHandlerMock._files[existingPrivateJwkPath] = existingPrivateJwk;
            fileHandlerMock._files[newPublicJwkPath] = newPublicJwk;

            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName, "--env", "dev",
                $"--{UpdateClientKeyParameterNames.ClientId.Long}", clientId,
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", existingPrivateJwkPath,
                $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", newPublicJwkPath
            };

            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);

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

        private static RootCommand CreateRootCommand(
            string[] args,
            FileHandlerMock fileHandlerMock,
            ILogger<ClientKeyUpdaterService> loggerMock,
            IHelseIdSelvbetjeningService helseIdServiceMock)
        {
            return Program.BuildRootCommand(new CommandInput()
            {
                Args = args,
                OverrideServices = services =>
                {
                    services.AddSingleton<IFileHandler>(fileHandlerMock);
                    services.AddSingleton(loggerMock);
                    services.AddSingleton(helseIdServiceMock);
                }
            });
        }
    }
}
