using Fhi.HelseIdSelvbetjening.CLI.Commands;
using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    /// <summary>
    /// Shared utilities for CLI integration tests to eliminate code duplication
    /// and provide consistent test setup across different test classes.
    /// </summary>
    internal static class CLITestUtilities
    {
        /// <summary>
        /// Creates a RootCommand configured for testing with mocked dependencies.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="fileHandlerMock">Mock file handler for simulating file operations</param>
        /// <param name="loggerMock">Mock logger for verification</param>
        /// <param name="helseIdServiceMock">Mock HelseId service for controlling responses</param>
        /// <returns>Configured RootCommand ready for testing</returns>
        internal static RootCommand CreateRootCommand(
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

        /// <summary>
        /// Creates a standard set of command arguments for ReadClientSecretExpiration with a direct JWK.
        /// </summary>
        /// <param name="clientId">Client ID to use</param>
        /// <param name="jwk">JWK string to use</param>
        /// <returns>Array of command line arguments</returns>
        internal static string[] CreateDirectJwkArgs(string clientId, string jwk)
        {
            return new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", jwk
            };
        }

        /// <summary>
        /// Creates a standard set of command arguments for ReadClientSecretExpiration with a JWK file path.
        /// </summary>
        /// <param name="clientId">Client ID to use</param>
        /// <param name="filePath">Path to JWK file</param>
        /// <returns>Array of command line arguments</returns>
        internal static string[] CreateFilePathArgs(string clientId, string filePath)
        {
            return new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", filePath
            };
        }

        /// <summary>
        /// Creates a standard set of command arguments for ReadClientSecretExpiration with both direct JWK and file path.
        /// </summary>
        /// <param name="clientId">Client ID to use</param>
        /// <param name="directJwk">Direct JWK string</param>
        /// <param name="filePath">Path to JWK file</param>
        /// <returns>Array of command line arguments</returns>
        internal static string[] CreateBothJwkArgs(string clientId, string directJwk, string filePath)
        {
            return new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", directJwk,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", filePath
            };
        }

        /// <summary>
        /// Validates that a string represents a valid Unix epoch timestamp.
        /// Unix epoch timestamps are typically 10 digits long for dates from 2001-2286.
        /// This validates that the string is a reasonable epoch timestamp (not too far in past/future).
        /// </summary>
        /// <param name="value">The string to validate as an epoch timestamp</param>
        /// <returns>True if the string is a valid epoch timestamp, false otherwise</returns>
        internal static bool IsValidEpochTimestamp(string value)
        {
            // Check if it's a valid 10-digit epoch timestamp
            if (!long.TryParse(value, out long epochSeconds))
                return false;
            
            // Reasonable range: January 1, 2000 (946684800) to December 31, 2099 (4102444799)
            // This avoids both very old dates and far future dates that would indicate an error
            return epochSeconds >= 946684800 && epochSeconds <= 4102444799;
        }

        /// <summary>
        /// Common test JWK samples for consistent testing across different test classes.
        /// </summary>
        internal static class TestJwkSamples
        {
            internal const string SimpleValidJwk = """{"kid":"test-kid","kty":"RSA","d":"test-d-value","n":"test-n-value","e":"AQAB"}""";

            internal const string PrettyFormattedJwk = """
                {
                  "kid": "test-kid",
                  "kty": "RSA",
                  "d": "test-d-value",
                  "n": "test-n-value",
                  "e": "AQAB"
                }
                """;

            internal const string CompactJwk = """{"d":"test-d-value","e":"AQAB","kid":"test-kid","kty":"RSA","n":"test-n-value"}""";

            internal const string JwkWithSpaces = """{ "kid": "test-kid", "kty": "RSA", "d": "test-data", "n": "test-modulus", "e": "AQAB" }""";

            internal const string ComplexJwkWithSpecialChars = """
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

            internal const string RealWorldJwk = """
                {
                  "d": "Q4x8XiZ3JKn0-ijW-H9plfw7QF4VLK43jHxYtPJvX6GcBuEk_rMedziQuqbBCZrK6aWVspnYS6dQtj33Z2TtSkXu2gy_1xR2nR8h9XeZ6h6QRbL9bj1Qxrk70ry7bXz5",
                  "e": "AQAB",
                  "kid": "-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4",
                  "kty": "RSA",
                  "n": "test-modulus-value-here-would-be-much-longer-in-real-world",
                  "use": "sig",
                  "alg": "RS256"
                }
                """;
        }
    }
}
