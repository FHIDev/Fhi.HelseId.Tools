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
        public async Task UpdateClientKey_WithValidParametersKeysFromPath_Ok()
        {
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();

            const string clientId = "test-client-id";
            const string existingPrivateJwkPath = "c:\\temp\\existing-private.json";
            const string newPublicJwkPath = "c:\\temp\\new-public.json";

            var fileHandlerMock = new FileHandlerMock();
            fileHandlerMock.Files[existingPrivateJwkPath] = existingPrivateJwkPath;
            fileHandlerMock.Files[newPublicJwkPath] = newPublicJwkPath;

            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName,
                $"--{UpdateClientKeyParameterNames.ClientId.Long}", clientId,
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", existingPrivateJwkPath,
                $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", newPublicJwkPath,
                $"--{UpdateClientKeyParameterNames.YesOption.Long}"
            };

            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0));
            }
            await helseIdServiceMock.Received(1).UpdateClientSecret(
                Arg.Is<ClientConfiguration>(c => c.ClientId == clientId),
                Arg.Any<string>());

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

        [Test]
        public void UpdateClientKeys_WithValidParametersKeysFromParameters_Ok()
        {
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();

            const string clientId = "test-client-id";
            const string existingPrivateJwk = "{\"kid\":\"test-kid\",\"kty\":\"RSA\",\"private\":\"key-data\"}";
            const string newPublicJwk = "{\"kid\":\"new-kid\",\"kty\":\"RSA\",\"public\":\"key-data\"}";

            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName,
                $"--{UpdateClientKeyParameterNames.ClientId.Long}", clientId,
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwk.Long}", existingPrivateJwk,
                $"--{UpdateClientKeyParameterNames.NewPublicJwk.Long}", newPublicJwk,
                $"--{UpdateClientKeyParameterNames.YesOption.Long}"
            };
            // TODO: Implement test logic for updating client keys from parameters
        }

        [Test]
        public void UpdateClientKeys_PromptForEnvironment_StopOnInput()
        {
            // TODO: Implement test logic for prompt
        }

        [TestCase("", "c:\\temp")]
        [TestCase("c:\\temp", "")]
        public async Task UpdateClientKey_EmptyNewKeyPathOrOldKeyPath_GiveErrorMessage(string newKeyPath, string oldkeyPath)
        {
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();

            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName,
                $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", newKeyPath,
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", oldkeyPath,
                $"--{UpdateClientKeyParameterNames.ClientId.Long}", "88d474a8-07df-4dc4-abb0-6b759c2b99ec",
                $"--{UpdateClientKeyParameterNames.YesOption.Long}"
            };
            var rootCommand = CreateRootCommand(args, new FileHandlerMock(), loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);

            loggerMock.Received(1).Log(
               LogLevel.Error,
               Arg.Any<EventId>(),
               Arg.Is<object>(o => o.ToString()!.Contains("Parameters empty.")),
               Arg.Any<Exception>(),
               Arg.Any<Func<object, Exception?, string>>());
        }

        [Test]
        [Ignore("Need to figure out how to set description when missing required option")]
        public async Task UpdateClientKey_MissingRequiredParameterClientId_GiveErrorMessage()
        {
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName,
                $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", "c:\\temp",
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", "c:\\temp",
                $"--{UpdateClientKeyParameterNames.YesOption.Long}"
            };
            var rootCommand = CreateRootCommand(args, new FileHandlerMock(), loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);

            var output = stringWriter.ToString();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Not.EqualTo(0));
                Assert.That(output, Does.Contain("Missing required parameter Client ID").IgnoreCase);
            }
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
