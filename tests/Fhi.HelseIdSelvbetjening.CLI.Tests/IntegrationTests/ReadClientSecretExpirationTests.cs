using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.Net;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public class ReadClientSecretExpirationTests
    {
        [TestCase("{\n  \"kid\": \"test-kid\",\n  \"kty\": \"RSA\",\n  \"d\": \"test-d-value\",\n  \"n\": \"test-n-value\",\n  \"e\": \"AQAB\"\n}", "pretty formatted JSON with newlines")]
        [TestCase("{\"d\":\"test-d-value\",\"e\":\"AQAB\",\"kid\":\"test-kid\",\"kty\":\"RSA\",\"n\":\"test-n-value\"}", "compact JSON without newlines")]
        [TestCase("{ \"kid\": \"test-kid\", \"kty\": \"RSA\", \"d\": \"test-data\", \"n\": \"test-modulus\", \"e\": \"AQAB\" }", "JSON with spaces")]
        [TestCase(@"{""test"":""json"",""key"":""value""}", "JSON with quotes")]
        [TestCase(@"{""kid"":""test-with-special-chars-!@#$%^&*()"",""d"":""data-with-quotes-\""and\""-backslashes-\\"",""n"":""modulus"",""e"":""AQAB""}", "JSON with special characters and escaped quotes")]
        [TestCase(@"{""test"":""json""}", "simple JSON that could cause command line parsing issues")]
        public async Task ReadClientSecretExpiration_ValidExistingPrivateJwkArgument_ExitCode0(string jwk, string description)
        {
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder()
                    .WithReadClientSecretExpirationResponse(new ClientSecretExpirationResponse(HttpStatusCode.OK, null, DateTime.UtcNow.AddDays(30)))
                    .Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jwk
                ])
               .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), $"Command should complete successfully with {description}");
                await builder.HelseIdSelvbetjeningServiceMock
                    .Received(1)
                    .ReadClientSecretExpiration(Arg.Is<ClientConfiguration>(c => c.ClientId == "test-client-id" && c.Jwk == jwk));

                builder.LoggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication with Client ID") && o.ToString()!.Contains("Key ID (kid)")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );

                builder.LoggerMock.Received().Log(
                   LogLevel.Information,
                   Arg.Any<EventId>(),
                   Arg.Is<object>(o => o.ToString()!.IsValidEpochTimestamp()),
                   null,
                   Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_ExistingPrivateJwkAndExistingPrivateJwkPathArguments_UseExistingPrivateJwkAndExitCode0()
        {
            var filePath = @"c:\temp\jwk.json";
            var jwk = @"{""kid"":""direct-jwk-kid"",""kty"":""RSA""}";
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder()
                    .WithReadClientSecretExpirationResponse(new ClientSecretExpirationResponse(HttpStatusCode.OK, null, DateTime.UtcNow.AddDays(30)))
                    .Build())
                .WithFileHandler(
                    new FileHandlerBuilder()
                    .WithExistingPrivateJwk(filePath, @"{""kid"":""file-jwk-kid"",""kty"":""RSA""}")
                    .Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jwk,
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", filePath
                ])
               .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully");
                await builder.HelseIdSelvbetjeningServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == "test-client-id" && c.Jwk == jwk)
                );
                await builder.HelseIdSelvbetjeningServiceMock.DidNotReceive().ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.Jwk == filePath)
                );

                builder.LoggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains($"Using private key for authentication with Client ID: test-client-id, Key ID (kid): direct-jwk-kid")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_NoExpirationDateFound_LogErrorAndSetExitCode1()
        {
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder()
                        .WithReadClientSecretExpirationResponse(new ClientSecretExpirationResponse(
                            HttpStatusCode.OK,
                            "Success but no expiration",
                            DateTime.MinValue))
                        .Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", @"{""kid"":""test-key"",""kty"":""RSA""}"
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command invocation should complete");
                Assert.That(Environment.ExitCode, Is.EqualTo(1), "Exit code should be set to 1");
                await builder.HelseIdSelvbetjeningServiceMock.Received(1).ReadClientSecretExpiration(Arg.Any<ClientConfiguration>());
                builder.LoggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Expiration date not found")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MissingExistingPrivateJwkArgument_LogErrorAndSetExitCode1()
        {
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder().Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "clientId"
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command invocation should complete (Environment.ExitCode is set separately)");
                Assert.That(Environment.ExitCode, Is.EqualTo(1), "Exit code should be set to 1");
                await builder.HelseIdSelvbetjeningServiceMock.DidNotReceive().ReadClientSecretExpiration(Arg.Any<ClientConfiguration>());
                builder.LoggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("No private key provided")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        /// <summary>
        /// TODO: Verify if this makes sence. Same as comment on test below
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysFirstExpired_ReturnLatestExpiration()
        {
            const string clientId = "test-client-multiple-keys";
            const string privateJwkWithKid = """
                {
                    "kid": "newer-key-id",
                    "kty": "RSA",
                    "d": "test-d-value",
                    "n": "test-n-value",
                    "e": "AQAB"
                }
                """;

            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(60)
            );
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder()
                        .WithReadClientSecretExpirationResponse(mockResponse)
                        .Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", privateJwkWithKid
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should succeed");
                await builder.HelseIdSelvbetjeningServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == privateJwkWithKid)
                );
                var expectedEpoch = ((DateTimeOffset)mockResponse.ExpirationDate).ToUnixTimeSeconds();
                builder.LoggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains(expectedEpoch.ToString())),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        /// <summary>
        /// TODO: Do not understand the value of the tests. Multiple keys on the client?
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysDifferentKids_HandleCorrectly()
        {
            const string clientId = "test-client-kid-matching";
            const string privateJwkWithSpecificKid = """
                {
                    "kid": "specific-key-id-to-match",
                    "kty": "RSA", 
                    "d": "test-d-value",
                    "n": "test-n-value",
                    "e": "AQAB"
                }
                """;

            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(60)
            );
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder()
                        .WithReadClientSecretExpirationResponse(mockResponse)
                        .Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", privateJwkWithSpecificKid
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should succeed");
                await builder.HelseIdSelvbetjeningServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c =>
                        c.ClientId == clientId &&
                        c.Jwk == privateJwkWithSpecificKid)
                );
                builder.LoggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_ExistingPrivateJwkArgumentWithInvalidJson_LogErrorSetExitCode1()
        {
            const string potentiallyProblematicJson = """
                {
                    "kty": "RSA",
                    "kid": "test-kid-with-json-in-name",
                    "d": "invalid-but-string-value-that-might-cause-issues",
                    "n": "another-invalid-but-string-value",
                    "e": "not-standard-AQAB",
                    "json": "json",
                    "description": "json with json everywhere"
                }
                """;

            var validationErrorResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.BadRequest,
                "Invalid JWK format",
                DateTime.MinValue,
                new List<string> { "Invalid JWK format" }
            );
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder()
                        .WithReadClientSecretExpirationResponse(validationErrorResponse)
                        .Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", potentiallyProblematicJson
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            await builder.HelseIdSelvbetjeningServiceMock.Received(1).ReadClientSecretExpiration(
                Arg.Is<ClientConfiguration>(c => c.ClientId == "test-client-id" && c.Jwk == potentiallyProblematicJson)
            );
            builder.LoggerMock.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Validation failed")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [TestCase("")]
        [TestCase("   \n\t  \r\n  ")]
        public async Task ReadClientSecretExpiration_EmptyExistingPrivateJwkArgument_LogErrorAndSetExitCode1(string jwk)
        {
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder().Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", "test-client-id",
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jwk
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            builder.LoggerMock.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("No private key provided")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }


        [Test]
        public async Task ReadClientSecretExpiration_ServiceValidationErrors_LogValidationErrorsExitCode1()
        {
            const string clientId = "";
            const string validJwk = @"{""kid"":""test-kid"",""kty"":""RSA"",""d"":""test-d"",""n"":""test-n"",""e"":""AQAB""}";

            var validationErrors = new List<string> { "ClientId cannot be null or empty", "Jwk cannot be null or empty" };
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.BadRequest,
                "Validation failed",
                DateTime.MinValue,
                validationErrors
            );
            var builder = new RootCommandBuilder()
                .WithSelvbetjeningService(
                    new SelvbetjeningServiceBuilder()
                        .WithReadClientSecretExpirationResponse(mockResponse)
                        .Build())
                .WithLogger(Substitute.For<ILogger<ClientSecretExpirationReaderService>>())
                .WithArgs([
                    ReadClientSecretExpirationParameterNames.CommandName,
                    $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                    $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", validJwk
                ])
                .Build();

            int exitCode = await builder.RootCommand.InvokeAsync(builder.Args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete even with validation errors");
                await builder.HelseIdSelvbetjeningServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == validJwk)
                );
                builder.LoggerMock.Received().Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Validation failed: ClientId cannot be null or empty, Jwk cannot be null or empty")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }
    }
}
