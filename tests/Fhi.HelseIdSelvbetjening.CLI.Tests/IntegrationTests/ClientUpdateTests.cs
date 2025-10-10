using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening.Dtos;
using Fhi.HelseIdSelvbetjening.UnitTests.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System.CommandLine;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public class ClientUpdateTests
    {
        [Test]
        public async Task UpdateClientKey_ValidJwkArgumentsFromPath_ExitCode0()
        {
            const string clientId = "test-client-id";
            const string existingPrivateJwkPath = "c:\\temp\\existing-private.json";
            const string newPublicJwkPath = "c:\\temp\\new-public.json";
            const string authorityUrl = "https://helseid-sts.test.nhn.no";
            const string baseAddress = "https://api.selvbetjening.test.nhn.no";

            var fakeLogProvider = new FakeLoggerProvider();
            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(
                [
                    UpdateClientKeyParameterNames.CommandName,
                    $"--{UpdateClientKeyParameterNames.ClientId.Long}", clientId,
                    $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", existingPrivateJwkPath,
                    $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", newPublicJwkPath,
                    $"--{UpdateClientKeyParameterNames.AuthorityUrl.Long}", authorityUrl,
                    $"--{UpdateClientKeyParameterNames.BaseAddress.Long}", baseAddress,
                    $"--{UpdateClientKeyParameterNames.YesOption.Long}"
                ])
                .WithFileHandler(new FileHandlerBuilder()
                    .WithExistingPrivateJwk(existingPrivateJwkPath, @"{""kid"":""file-jwk-kid"",""kty"":""RSA""}")
                    .WithNewPublicJwk(newPublicJwkPath, @"{""kid"":""file-jwk-kid"",""kty"":""RSA""}")
                    .Build())
                .WithSelvbetjeningService(new HelseIdSelvbetjeningServiceBuilder()
                               .WithDPopTokenResponse(new TokenResponse("access_token", false, null))
                               .WithUpdateClientSecretResponse(new ClientSecretUpdateResult("2028-08-08T00:00:00Z")).Build())
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0));
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain("Expiration Date: ClientSecretUpdateResult { Expiration = 2028-08-08T00:00:00Z }"));
                Assert.That(logs!, Does.Contain("Keys successfully updated.")); 
            }
        }

        [Test]
        public async Task UpdateClientKeys_ValidJwkArgumentsFromParameters_ExitCode0()
        {
            const string clientId = "test-client-id";
            const string existingPrivateJwk = "{\"kid\":\"test-kid\",\"kty\":\"RSA\",\"private\":\"key-data\"}";
            const string newPublicJwk = "{\"kid\":\"new-kid\",\"kty\":\"RSA\",\"public\":\"key-data\"}";
            const string authorityUrl = "https://helseid-sts.test.nhn.no";
            const string baseAddress = "https://api.selvbetjening.test.nhn.no";
            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName,
                $"--{UpdateClientKeyParameterNames.ClientId.Long}", clientId,
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwk.Long}", existingPrivateJwk,
                $"--{UpdateClientKeyParameterNames.NewPublicJwk.Long}", newPublicJwk,
                $"--{UpdateClientKeyParameterNames.AuthorityUrl.Long}", authorityUrl,
                $"--{UpdateClientKeyParameterNames.BaseAddress.Long}", baseAddress,
                $"--{UpdateClientKeyParameterNames.YesOption.Long}"
            };

            var fakeLogProvider = new FakeLoggerProvider();
            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(args)
                .WithFileHandler(new FileHandlerBuilder()
                    .Build())
                .WithSelvbetjeningService(new HelseIdSelvbetjeningServiceBuilder()
                               .WithDPopTokenResponse(new TokenResponse("access_token", false, null))
                               .WithUpdateClientSecretResponse(new ClientSecretUpdateResult("2028-08-08T00:00:00Z")).Build())
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0));
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain("Expiration Date: ClientSecretUpdateResult { Expiration = 2028-08-08T00:00:00Z }"));
                Assert.That(logs!, Does.Contain("Keys successfully updated.")); 
            }
        }

        [TestCase("", "")]
        [TestCase("", "c:\\temp")]
        [TestCase("c:\\temp", "")]
        public async Task UpdateClientKey_EmptyPathJwkArguments_LogErrorAndExitCode1(string newKeyPath, string oldKeyPath)
        {
            var selvbetjeningsApi = Substitute.For<ISelvbetjeningApi>();
            selvbetjeningsApi
                .UpdateClientSecretsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns((new ClientSecretUpdateResult(""), null));
            var fakeLogProvider = new FakeLoggerProvider();

            var rootCommandBuilder = new RootCommandBuilder()
                .WithSelvbetjeningService(new HelseIdSelvbetjeningServiceBuilder()
                               .WithDPopTokenResponse(new TokenResponse("access_token", false, null))
                               .WithUpdateClientSecretResponse(new ClientSecretUpdateResult("")).Build())
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithFileHandler(new FileHandlerBuilder()
                    .Build())
                .WithArgs(
                [
                    UpdateClientKeyParameterNames.CommandName,
                    $"--{UpdateClientKeyParameterNames.ClientId.Long}", "88d474a8-07df-4dc4-abb0-6b759c2b99ec",
                    $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", newKeyPath,
                    $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", oldKeyPath,
                    $"--{UpdateClientKeyParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                    $"--{UpdateClientKeyParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no",
                    $"--{UpdateClientKeyParameterNames.YesOption.Long}"
                ]);

            var rootCommand = rootCommandBuilder.Build();

            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(1));
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!.Any(l => l.Contains("One or more parameters empty.")));
            }
        }

        [Test]
        [Ignore("Need to figure out how to set description when missing required option")]
        public async Task UpdateClientKey_MissingRequiredParameterClientId_GiveErrorMessage()
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var rootCommandBuilder = new RootCommandBuilder()
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithSelvbetjeningService(new HelseIdSelvbetjeningServiceBuilder()
                               .WithDPopTokenResponse(new TokenResponse("access_token", false, null))
                               .WithUpdateClientSecretResponse(new ClientSecretUpdateResult("")).Build())
                .WithArgs(
                [
                    UpdateClientKeyParameterNames.CommandName,
                    $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", "c:\\temp",
                    $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", "c:\\temp",
                    $"--{UpdateClientKeyParameterNames.YesOption.Long}"
                ]);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Not.EqualTo(1));
                Assert.That(fakeLogProvider.Collector.LatestRecord.Message, Does.Contain("Missing required parameter Client ID").IgnoreCase);
            }
        }
    }
}
