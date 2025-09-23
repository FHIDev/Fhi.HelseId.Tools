using Fhi.HelseIdSelvbetjening.Business;
using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening.Dtos;
using Fhi.HelseIdSelvbetjening.UnitTests.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public class ReadClientSecretExpirationTests
    {
        [TestCase("{\n  \"kid\": \"test-kid\",\n  \"kty\": \"RSA\",\n  \"d\": \"test-d-value\",\n  \"n\": \"test-n-value\",\n  \"e\": \"AQAB\"\n}")]
        [TestCase("{\"d\":\"test-kid\",\"e\":\"AQAB\",\"kid\":\"test-kid\",\"kty\":\"RSA\",\"n\":\"test-n-value\"}")]
        [TestCase("{ \"kid\": \"test-kid\", \"kty\": \"RSA\", \"d\": \"test-data\", \"n\": \"test-modulus\", \"e\": \"AQAB\" }")]
        // [TestCase(@"{""kid"":""test-with-special-chars-!@#$%^&*()"",""d"":""data-with-quotes-\""and\""-backslashes-\\"",""n"":""modulus"",""e"":""AQAB""}")]
        public async Task ReadClientSecretExpiration_ValidDirectJwkArgument_ExitCode0(string jwk)
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var clientSecrets = new List<ClientSecret>()
            {
                new() { Expiration = DateTime.Parse("2027-06-20T00:00:00Z"), Kid = "test-kid", Origin = "Api" }
            };
            var rootCommandBuilder = new RootCommandBuilder()
                .WithSelvbetjeningService(CreateSelvbetjeningService(clientSecrets))
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jwk,
                    $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                    $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no"
                ]);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(fakeLogProvider.Collector?.LatestRecord.Message, Does.Contain(((DateTimeOffset)clientSecrets.FirstOrDefault()!.Expiration!).ToUnixTimeSeconds().ToString()));
                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain("Kid: test-kid"));
            }
        }

        /// <summary>
        /// TODO: should return input validation error? or warning that both ExistingPrivateJwk and ExistingPrivateJwkPath are provided.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ReadClientSecretExpiration_JwkFromFileAndDirectJwkArgument_UseDirectJwkArgument()
        {
            var filePath = @"c:\temp\jwk.json";
            var jwk = @"{""kid"":""direct-jwk-kid"",""kty"":""RSA""}";
            var expiration = DateTime.Parse("2028-08-08T00:00:00Z");
            var fakeLogProvider = new FakeLoggerProvider();
            var rootCommandBuilder = new RootCommandBuilder()
                .WithSelvbetjeningService(CreateSelvbetjeningService([
                    new() { Expiration = expiration, Kid = "direct-jwk-kid", Origin = "Api" },
                    new() { Expiration = DateTime.Parse("2027-06-20T00:00:00Z"), Kid = "file-jwk-kid", Origin = "Api" }
                ]))
                .WithFileHandler(
                    new FileHandlerBuilder()
                    .WithExistingPrivateJwk(filePath, @"{""kid"":""file-jwk-kid"",""kty"":""RSA""}")
                    .Build())
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jwk,
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", filePath,
                    $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                    $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no"
                ]);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully");
                Assert.That(logs!, Does.Contain(((DateTimeOffset)expiration).ToUnixTimeSeconds().ToString()));
                Assert.That(logs!, Does.Contain("Kid: direct-jwk-kid"));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_NoExpirationDateFound_LogErrorAndSetExitCode1()
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var rootCommandBuilder = new RootCommandBuilder()
                .WithSelvbetjeningService(CreateSelvbetjeningService([]))
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", @"{""kid"":""test-kid"",""kty"":""RSA""}",
                    $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                    $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no"
                ]);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(1), "Command invocation should complete");
                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain("No secret found with matching Kid."));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MissingJwkArgument_LogErrorAndSetExitCode1()
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var rootCommandBuilder = new RootCommandBuilder()
                .WithSelvbetjeningService(CreateSelvbetjeningService([]))
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "clientId",
                    $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                    $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no"
                ]);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(1));
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain("No private key provided. Either ExistingPrivateJwk or ExistingPrivateJwkPath must be specified."));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeys_ReturnKeyWithMatchingKid_ReturnMissingKid()
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var clientsecretResponse = new List<ClientSecret>()
            {
                new() { Expiration = DateTime.Now.AddDays(60), Kid = "kid-one" },
                new() { Expiration = DateTime.Now.AddDays(30), Kid = "kid-two" }
            };
            var rootCommandBuilder = new RootCommandBuilder()
                .WithSelvbetjeningService(CreateSelvbetjeningService(clientsecretResponse))
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-multiple-keys",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", """
                {
                    "kid": "kid-two",
                    "kty": "RSA",
                    "d": "test-d-value",
                    "n": "test-n-value",
                    "e": "AQAB"
                }
                """,
                $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no"
                ]);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0));
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain("Kid: kid-two"));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysNoKidInClientJwk_ReturnMissingKid()
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var clientsecretResponse = new List<ClientSecret>()
            {
                new() { Expiration = DateTime.Now.AddDays(60), Kid = "kid-one" },
                new() { Expiration = DateTime.Now.AddDays(30), Kid = "kid-two" }
            };
            var rootCommandBuilder = new RootCommandBuilder()
                .WithSelvbetjeningService(CreateSelvbetjeningService(clientsecretResponse))
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-multiple-keys",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", """
                {
                    "kty": "RSA",
                    "d": "test-d-value",
                    "n": "test-n-value",
                    "e": "AQAB"
                }
                """,
                $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no"
                ]);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(1));
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain("No secret found with matching Kid."));
            }
        }

        [TestCase("")]
        [TestCase("   \n\t  \r\n  ")]
        public async Task ReadClientSecretExpiration_EmptyJwkArgument_LogErrorAndSetExitCode1(string jwk)
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var rootCommandBuilder = new RootCommandBuilder()
                .WithSelvbetjeningService(CreateSelvbetjeningService([new ClientSecret() { Expiration = DateTime.Now, Kid = "kid" }]))
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace)
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jwk,
                    $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                    $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no",
                ]);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(1));
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain("No private key provided. Either ExistingPrivateJwk or ExistingPrivateJwkPath must be specified."));
            }
        }

        private static HelseIdSelvbetjeningService CreateSelvbetjeningService(List<ClientSecret> clientSecrets) => new HelseIdSelvbetjeningServiceBuilder()
                               .WithDPopTokenResponse(new TokenResponse("access_token", false, null))
                               .WithGetClientSecretResponse(clientSecrets).Build();
    }
}
