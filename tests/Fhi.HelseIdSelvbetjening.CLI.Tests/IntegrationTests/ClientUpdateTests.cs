using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public partial class ClientUpdateTests
    {
        [Test]
        public async Task UpdateClientKey_ValidJwkArgumentsFromPath_ExitCode0()
        {
            const string clientId = "test-client-id";
            const string existingPrivateJwkPath = "c:\\temp\\existing-private.json";
            const string newPublicJwkPath = "c:\\temp\\new-public.json";
            var builder = new RootCommandBuilder()
                .WithArgs(
                [
                    UpdateClientKeyParameterNames.CommandName,
                    $"--{UpdateClientKeyParameterNames.ClientId.Long}", clientId,
                    $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", existingPrivateJwkPath,
                    $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", newPublicJwkPath,
                    $"--{UpdateClientKeyParameterNames.YesOption.Long}"
                ])
                .WithFileHandler(new FileHandlerBuilder()
                    .WithExistingPrivateJwk(existingPrivateJwkPath)
                    .WithNewPublicJwk(newPublicJwkPath)
                    .Build())
                .WithSelvbetjeningService(Substitute.For<IHelseIdSelvbetjeningService>())
                .WithLogger(Substitute.For<ILogger<ClientKeyUpdaterService>>())
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            Assert.That(exitCode, Is.EqualTo(0));
            await builder.HelseIdSelvbetjeningServiceMock.Received(1).UpdateClientSecret(
                Arg.Is<ClientConfiguration>(c => c.ClientId == clientId),
                Arg.Any<string>());
            builder.LoggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains($"Update client {clientId}")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
            builder.LoggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("NewKey:")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
            builder.LoggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("OldKey:")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Test]
        public void UpdateClientKeys_ValidJwkArgumentsFromParameters_ExitCode0()
        {
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
            var builder = new RootCommandBuilder().WithArgs(args).Build();

            // TODO: Implement test logic for updating client keys from parameters
        }

        [Test]
        public void UpdateClientKeys_PromptForEnvironment_StopOnInput()
        {
            // TODO: Implement test logic for prompt
        }

        [TestCase("", "c:\\temp")]
        [TestCase("c:\\temp", "")]
        public async Task UpdateClientKey_EmptyJwkArguments_LogErrorAndExitCode1(string newKeyPath, string oldKeyPath)
        {
            var builder = new RootCommandBuilder()
                .WithLogger(Substitute.For<ILogger<ClientKeyUpdaterService>>())
                .WithSelvbetjeningService(Substitute.For<IHelseIdSelvbetjeningService>())
                .WithFileHandler(new FileHandlerBuilder()
                    .Build())
                .WithArgs(
                [
                    UpdateClientKeyParameterNames.CommandName,
                    $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", newKeyPath,
                    $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", oldKeyPath,
                    $"--{UpdateClientKeyParameterNames.ClientId.Long}", "88d474a8-07df-4dc4-abb0-6b759c2b99ec",
                    $"--{UpdateClientKeyParameterNames.YesOption.Long}"
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            builder.LoggerMock.Received(1).Log(
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
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var builder = new RootCommandBuilder()
                .WithLogger(Substitute.For<ILogger<ClientKeyUpdaterService>>())
                .WithSelvbetjeningService(Substitute.For<IHelseIdSelvbetjeningService>())
                .WithArgs(
                [
                    UpdateClientKeyParameterNames.CommandName,
                    $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", "c:\\temp",
                    $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", "c:\\temp",
                    $"--{UpdateClientKeyParameterNames.YesOption.Long}"
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);
            var output = stringWriter.ToString();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Not.EqualTo(0));
                Assert.That(output, Does.Contain("Missing required parameter Client ID").IgnoreCase);
            }
        }
    }
}
