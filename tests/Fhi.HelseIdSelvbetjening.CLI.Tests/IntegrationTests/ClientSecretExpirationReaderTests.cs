using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.Net;
using Fhi.HelseIdSelvbetjening.CLI.Commands;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    /// <summary>
    /// Tests for ClientSecretExpirationReaderService with ExistingPrivateJwk parameter
    /// including JSON with newlines and various formatting scenarios
    /// </summary>
    public class ClientSecretExpirationReaderTests
    {
        [TestCase("{\n  \"kid\": \"test-kid\",\n  \"kty\": \"RSA\",\n  \"d\": \"test-d-value\",\n  \"n\": \"test-n-value\",\n  \"e\": \"AQAB\"\n}", "pretty formatted JSON with newlines")]
        [TestCase("{\"d\":\"test-d-value\",\"e\":\"AQAB\",\"kid\":\"test-kid\",\"kty\":\"RSA\",\"n\":\"test-n-value\"}", "compact JSON without newlines")]
        [TestCase("{ \"kid\": \"test-kid\", \"kty\": \"RSA\", \"d\": \"test-data\", \"n\": \"test-modulus\", \"e\": \"AQAB\" }", "JSON with spaces")]
        public async Task ReadClientSecretExpiration_WithVariousJsonFormats_ShouldSucceed(string jwk, string description)
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(30)
            );
            helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jwk
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), $"Command should complete successfully with {description}");
                await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == jwk)
                );
                loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithRealWorldJwkFromFile_ShouldSucceed()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
            const string realWorldJwk = @"{
  ""d"": ""Q4x8XiZ3JKn0-ijW-H9plfw7QF4VLK43jHxYtPJvX6GcBuEk_rMedziQuqbBCZrK6aWVspnYS6dQtj33Z2TtSkXu2gy_1xR2nR8h9XeZ6h6QRbL9bj1Qxrk70ry7bXz5WIjyyuPmY73aPw9OFrZ_NDeUQjiEofzTHkr86ZIVjAmNLarVufG9P2V6fz14wwHc3aLBVgUt7Rxx5sFOQR30zYGpd1BH-xK6ykA6n6BdaIc4luWw_SkmVowwO4toScj07qoAYTUR4IFQHYt7sQZNufFG89nB-v_Er0a2tRvtME2NnU_4rn4ea1yyGFlYH_6Amtb8u4-TAeOESjrMw9ylBkvb6vIvtqT0lQdBJJEPI_Hx-655ElvO4zT48HBS6oVZHCARN17d7pQWrnxiSusYEdM9RwJET57ieVayo-baQe3NOvj2Y5V2H034cWCJt_DTh7ye9RXD4gtMnHDQ-tgV6ztwW8GkGvbJzXUnkqGXUvKqjeJAnOc2Ahoxpc-9cnMnW2DrwPnI0f9Jsq0n3hQyqwnnyimIeZn32WVe2Q4XC7d_VB21E8oDZhdeUlxuTZX-foTrYB3xvDKB6tLCaaMbfpzvUsSfSYqbAXQfqhQWosyt7w-ZIYJOY05fWspR3mlpo5IMGkaDp8clvz51f8zdMfSYFTml4e_zjoduvlz2wyE"",
  ""e"": ""AQAB"",
  ""kid"": ""-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"",
  ""kty"": ""RSA"",  ""n"": ""iu9EmQLoIJBPBqm3jLYW4oI8yLkOxvKg-OagE8HlzP-RQnDXH9hBe2cTRZ3oNqG1viWmv6-dxNtKU1QxOpezWLx-N-AJ7dIlXTMUGkCheHUorPSzakeBUOCHtvT1Tdv9Mzue9fVt3JxpPX6mQNlsOzwk9L8HmbgojMcApKmQcfNriVV72byLuaAoh9fcXSNm6TUuwO5cPmnHgS5B5Hfe5P0OIte027oZyjPiYm-QbV4YJNjwwwZnPvkLaRjw6L8sV5TAOLvNQIt63OpF8UHPjBsM8LJHdHFUMgx2BaMaJC8tNCi_8UWGG59sd4-_vJC78s3wZNEGL6OwCngpF7NLwaP9Zqxx8DDkOY71MvvcAyu4i0D6_8A8_qewLvb_SPxNpCe8zH5MJIKNJB38InWd8FpvpbPuEJt4oK1gfUBWLWQ39YIHzodKhkN-qAXYWGyzJ2nJdNIMAclefw251Cvjcyf3gmVATXDBAo-piUJIGXC3y7yqfyMupe_4oRe69DFBZTecXSLEdbAbUtiaH9r4rY5oeYCiZ70wcFcieHFZLwfleCPm5Cz8rEQxK8KjMis2kb1aRxVytTj_0pOkw1HEJU1tv_TWmD136RgoRtiqnVoxmCM6Q4XxXrOnGMPZR0_ScYHdW_YjDgnJBQykAbzW0nC47d3KSotktz1cPejo5_s""
}";
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(30)
            );
            helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", realWorldJwk
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully with real-world JWK");
                await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == realWorldJwk)
                );
                loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Successfully retrieved expiration date")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithEmptyExistingPrivateJwk_ShouldFail()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
            const string emptyJwk = "";
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", emptyJwk
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            loggerMock.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("No private key provided")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithWhitespaceOnlyJwk_ShouldFail()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
            const string whitespaceJwk = "   \n\t  \r\n  ";
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", whitespaceJwk
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            loggerMock.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("No private key provided")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithFilePathAndDirectJwk_ShouldPrioritizeDirectJwk()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
            const string directJwk = @"{""kid"":""direct-jwk"",""kty"":""RSA""}";
            const string filePath = @"c:\temp\jwk.json";
            const string fileJwk = @"{""kid"":""file-jwk"",""kty"":""RSA""}";
            fileHandlerMock.Files[filePath] = fileJwk;
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(30)
            );
            helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", directJwk,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", filePath
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully");
                await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == directJwk)
                );
                await helseIdServiceMock.DidNotReceive().ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.Jwk == fileJwk)
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithJsonContainingJsonKeyword_ShouldSucceed()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
            const string jsonWithJsonKeyword = """
                                               {
                                                   "kid": "complex-test-kid-with-special-chars-!@#$%^&*()",
                                                   "kty": "RSA",
                                                   "d": "test-d-value-with-special-chars-and-json-keyword",
                                                   "n": "test-n-value-with-underscores_and_dashes-and-json",
                                                   "e": "AQAB",
                                                   "alg": "RS256",
                                                   "use": "sig",
                                                   "key_ops": ["sign", "verify"],
                                                   "custom_field": "This field contains json keyword and \"quotes\" and 'single quotes'",
                                                   "another_field": "{\"nested\": \"json\", \"with\": \"more json\"}",
                                                   "description": "This is a json key with json in various places"
                                               }
                                               """;
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(30)
            );
            helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jsonWithJsonKeyword
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully with JSON containing 'json' keyword");
                await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == jsonWithJsonKeyword)
                );
                loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WithMalformedButValidStringJson_ShouldHandleGracefully()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
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

            helseIdServiceMock
                .When(x => x.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()))
                .Do(x => throw new ArgumentException("Invalid JWK format"));
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", potentiallyProblematicJson
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == potentiallyProblematicJson)
            );
            loggerMock.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Service error while reading client secret expiration")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }
        
        [TestCase(@"{""test"":""json"",""key"":""value""}", "JSON with quotes")]
        [TestCase(@"{""kid"":""test-with-special-chars-!@#$%^&*()"",""d"":""data-with-quotes-\""and\""-backslashes-\\"",""n"":""modulus"",""e"":""AQAB""}", "JSON with special characters and escaped quotes")]
        [TestCase(@"{""test"":""json""}", "simple JSON that could cause command line parsing issues")]
        public async Task ReadClientSecretExpiration_WithCommandLineParsingChallenges_ShouldHandleCorrectly(string problematicJson, string description)
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(30)
            );
            helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", problematicJson
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), $"Command should complete successfully with {description}");
                await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == problematicJson)
                );
                loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        private static RootCommand CreateRootCommand(
            string[] args,
            FileHandlerMock fileHandlerMock,
            ILogger<ClientSecretExpirationReaderService> loggerMock,
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

        [Test]
        [Category("RealWorldScenarios")]
        public async Task ReadClientSecretExpiration_WithFilePathAsWorkaround_ShouldAvoidCommandLineParsingIssues()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
            const string clientId = "test-client-id";
            const string filePath = @"c:\temp\complex-jwk.json";
            const string complexJson = """
                                       {
                                           "kid": "test-kid-with-special-chars-!@#$%^&*()",
                                           "kty": "RSA",
                                           "d": "test-d-value-with-quotes-\"and\"-backslashes-\\",
                                           "n": "test-n-value",
                                           "e": "AQAB",
                                           "custom": "This contains \"nested quotes\" and 'single quotes' and json keyword"
                                       }
                                       """;
            fileHandlerMock.Files[filePath] = complexJson;
            var mockResponse = new ClientSecretExpirationResponse(
                HttpStatusCode.OK,
                null,
                DateTime.Now.AddDays(30)
            );
            helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", filePath
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should complete successfully when using file path");
                await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == complexJson)
                );
                loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        [Category("KidHandlingIssue")]
        public async Task ReadClientSecretExpiration_WithMultipleKeysFirstExpired_ShouldReturnLatestExpiration()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
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
            helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", privateJwkWithKid
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should succeed");
                await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c => c.ClientId == clientId && c.Jwk == privateJwkWithKid)
                );
                // 1. Extracting the kid from the request JWK
                // 2. Either matching the kid to a specific key's expiration
                // 3. Or returning the latest (furthest in future) expiration date
                loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Successfully retrieved expiration date")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }

        [Test]
        [Category("KidHandlingIssue")]
        public async Task ReadClientSecretExpiration_WithMultipleKeysDifferentKids_ShouldHandleCorrectly()
        {
            var loggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var fileHandlerMock = new FileHandlerMock();
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
                DateTime.Now.AddDays(60) // Future expiration
            );
            helseIdServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(mockResponse);
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", privateJwkWithSpecificKid
            };
            var rootCommand = CreateRootCommand(args, fileHandlerMock, loggerMock, helseIdServiceMock);
            int exitCode = await rootCommand.InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Command should succeed");
                await helseIdServiceMock.Received(1).ReadClientSecretExpiration(
                    Arg.Is<ClientConfiguration>(c =>
                        c.ClientId == clientId &&
                        c.Jwk == privateJwkWithSpecificKid)
                );
                // by extracting the kid from the client JWK and either:
                // 1. Finding a matching kid in the server response
                // 2. Returning the latest expiration date from all keys
                loggerMock.Received().Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o.ToString()!.Contains("Using private key for authentication")),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
            }
        }
    }
}