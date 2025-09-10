using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateJsonWebKey;
using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;

namespace Fhi.HelseIdSelvbetjening.CLI.AcceptanceTests
{
    /// <summary>
    /// Manual acceptance tests for the CLI. These tests should be run against test environment.
    /// </summary>
    [TestFixture, Explicit]
    public class AcceptanceTests
    {
        /// <summary>
        /// In order to run this test:
        /// 1. Set directory to where new keys should be stored
        /// 2. Set directory to where existing (old) keys is stored
        /// 3. Set clientId
        /// 
        /// Note: In order to update secret the nhn:selvbetjening/client scope must be set on the client
        /// </summary>
        /// <returns></returns>
        [Test, Explicit("This test generates keys and update client with new keys should be run manually.")]
        public async Task GenerateJsonWebKeys_And_UpdateClientKeysFromPath()
        {
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            /******************************************************************************************
            * Generate new keys
            *****************************************************************************************/

            var keyDirectory = Path.Combine(Environment.CurrentDirectory, "TestData");
            var keyPrefix = "manualtest";
            int exitCodeGenerateJsonWebKeys = await Program.Main([
                GenerateJsonWebKeyParameterNames.CommandName,
                $"--{GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Long}", keyPrefix,
                $"--{GenerateJsonWebKeyParameterNames.KeyDirectory.Long}", keyDirectory
            ]);

            var output = stringWriter.ToString();
            Assert.That(exitCodeGenerateJsonWebKeys, Is.EqualTo(0), "Generation of keys succeeded");

            /******************************************************************************************
             * Update Client with new keys
             *****************************************************************************************/
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var oldkeyPath = Path.Combine(Environment.CurrentDirectory, "TestData") + "\\oldkey.json";
            var clientId = "88d474a8-07df-4dc4-abb0-6b759c2b99ec";
            var exitCodeUpdateClient = await Program.Main(
            [
                UpdateClientKeyParameterNames.CommandName,
                $"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", keyDirectory + "/" + keyPrefix + "_public.json",
                $"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", oldkeyPath,
                $"--{UpdateClientKeyParameterNames.ClientId.Long}",clientId,
                $"--{UpdateClientKeyParameterNames.YesOption.Long}"
            ]);

            output = stringWriter.ToString();
            Assert.That(exitCodeUpdateClient, Is.EqualTo(0), "Update of client keys succeeded");
        }

        /// <summary>
        /// In order to run this test:
        /// 1. Set directory to where existing (old) keys is stored
        /// 2. Set clientId to a valid test client
        /// 
        /// Note: In order to read secret expiration the nhn:selvbetjening/client scope must be set on the client
        /// 
        /// Setup Instructions:
        /// 1. Configure Test Client ID in this file (replace the clientId below)
        /// 2. Create a TestData directory in the test project root (not in bin folder)
        /// 3. Add your private key file as TestData/oldkey.json
        /// 4. The test will automatically set DOTNET_ENVIRONMENT=Test
        /// 
        /// Expected Results:
        /// - Success: Exit code 0, Output contains "Reading client secret expiration for client"
        /// - Failure: Non-zero exit code with error message
        /// </summary>
        /// <returns></returns>        
        [Test, Explicit("This test reads client secret expiration and should be run manually against test environment.")]
        public async Task ReadClientSecretExpiration_FromPath()
        {
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            /******************************************************************************************
             * Read Client Secret Expiration
             *****************************************************************************************/
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var testProjectDirectory = GetTestProjectDirectory();
            var existingKeyPath = Path.Combine(testProjectDirectory, "AcceptanceTests", "TestData", "oldkey.json");
            var clientId = "88d474a8-07df-4dc4-abb0-6b759c2b99ec"; // Replace with your test client ID
            if (!File.Exists(existingKeyPath))
            {
                Assert.Fail($"Test key file not found at: {existingKeyPath}\n" +
                           $"Please ensure your test client's private key is available at TestData/oldkey.json.\n" +
                           $"Test project directory: {testProjectDirectory}");
            }
            int exitCode = await Program.Main(
            [
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", existingKeyPath
            ]);

            var output = stringWriter.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Reading client secret expiration succeeded");
                Assert.That(output, Does.Contain("Reading client secret expiration for client"), "Output contains expected message");
            }
        }

        /// <summary>
        /// In order to run this test:
        /// 1. Set clientId to a valid test client
        /// 2. Set existingPrivateJwk to a valid private key JSON
        /// 
        /// Note: In order to read secret expiration the nhn:selvbetjening/client scope must be set on the client
        /// 
        /// Setup Instructions:
        /// 1. Configure Test Client ID in this file (replace the clientId below)
        /// 2. Replace the existingPrivateJwk with a valid private key JSON
        /// 3. The test will automatically set DOTNET_ENVIRONMENT=Test
        /// 
        /// Format for the private key:
        /// {
        ///   "kty": "RSA",
        ///   "d": "...",
        ///   "n": "...",
        ///   "e": "AQAB",
        ///   "use": "sig",
        ///   "kid": "..."
        /// }
        /// 
        /// Expected Results:
        /// - Success: Exit code 0, Output contains "Reading client secret expiration for client"
        /// - Failure: Non-zero exit code with error message
        /// </summary>
        /// <returns></returns>
        [Test, Explicit("This test reads client secret expiration using direct key value and should be run manually against test environment.")]
        public async Task ReadClientSecretExpiration_FromDirectKey()
        {
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            /******************************************************************************************
             * Read Client Secret Expiration using direct key value
             *****************************************************************************************/
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var clientId = "88d474a8-07df-4dc4-abb0-6b759c2b99ec"; // Replace with valid test client ID
            var existingPrivateJwk = "{\"kty\":\"RSA\",\"d\":\"...\",\"n\":\"...\",\"e\":\"AQAB\"}"; // Replace with valid private key

            int exitCode = await Program.Main(
            [
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", existingPrivateJwk
            ]);

            var output = stringWriter.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Reading client secret expiration succeeded");
                Assert.That(output, Does.Contain("Reading client secret expiration for client"), "Output contains expected message");
            }
        }

        private static string GetTestProjectDirectory()
        {
            // Start from the current directory (bin/Debug/net9.0) and navigate back to the test project root
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);

            // Navigate up until we find the .csproj file or reach a reasonable limit
            while (currentDirectory != null && currentDirectory.Parent != null)
            {
                var csprojFiles = currentDirectory.GetFiles("*.csproj");
                if (csprojFiles.Length > 0)
                {
                    return currentDirectory.FullName;
                }
                currentDirectory = currentDirectory.Parent;
            }

            // Fallback: assume we're in bin/Debug/net9.0 and go up 3 levels
            var fallbackPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
            return Path.GetFullPath(fallbackPath);
        }
    }
}
