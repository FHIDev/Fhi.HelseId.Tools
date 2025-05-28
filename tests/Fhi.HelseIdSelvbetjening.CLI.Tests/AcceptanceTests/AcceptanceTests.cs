using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey;
using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.CLI.Tests.Utilities;

namespace Fhi.HelseIdSelvbetjening.CLI.AcceptanceTests
{
    /// <summary>
    /// Manual acceptance tests for the CLI. These tests should be run against test environment.
    /// </summary>
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
        [Test]
        [Explicit("This test generates keys and update client with new keys should be run manually.")]
        public async Task GenerateKeys_And_UpdateClientKeysFromPath()
        {
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            /******************************************************************************************
            * Generate new keys
            *****************************************************************************************/

            var keyDirectory = Path.Combine(Environment.CurrentDirectory, "TestData");
            var keyPrefix = "manualtest";
            int exitCodeGenerateKeys = await Program.Main([
                GenerateKeyParameterNames.CommandName,
                $"--{GenerateKeyParameterNames.KeyFileNamePrefix.Long}", keyPrefix,
                $"--{GenerateKeyParameterNames.KeyDirectory.Long}", keyDirectory
            ]);

            var output = stringWriter.ToString();
            Assert.That(exitCodeGenerateKeys, Is.EqualTo(0), "Generation of keys succeeded");

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
        /// </summary>
        /// <returns></returns>        
        [Test]
        [Explicit("This test reads client secret expiration and should be run manually against test environment.")]
        public async Task ReadClientSecretExpiration_FromPath()
        {
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            /******************************************************************************************
             * Read Client Secret Expiration
             *****************************************************************************************/
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");            
            var testProjectDirectory = TestPathHelper.GetTestProjectDirectory();
            var existingKeyPath = Path.Combine(testProjectDirectory, "AcceptanceTests", "TestData", "oldkey.json");
            var clientId = "88d474a8-07df-4dc4-abb0-6b759c2b99ec";
            if (!File.Exists(existingKeyPath))
            {
                Assert.Fail($"Test key file not found at: {existingKeyPath}\n" +
                           $"Please ensure your test client's private key is available at TestData/oldkey.json.\n" +
                           $"Test project directory: {testProjectDirectory}");
            }            int exitCode = await Program.Main(
            [
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", existingKeyPath
            ]);

            var output = stringWriter.ToString();
            
            Assert.That(exitCode, Is.EqualTo(0), "Reading client secret expiration succeeded");
            Assert.That(output, Does.Contain("Reading client secret expiration for client"), "Output contains expected message");
        }

        /// <summary>
        /// In order to run this test:
        /// 1. Set clientId to a valid test client
        /// 2. Set existingPrivateJwk to a valid private key JSON
        /// 
        /// Note: In order to read secret expiration the nhn:selvbetjening/client scope must be set on the client
        /// </summary>
        /// <returns></returns>
        [Test]
        [Explicit("This test reads client secret expiration using direct key value and should be run manually against test environment.")]
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
              
            Assert.That(exitCode, Is.EqualTo(0), "Reading client secret expiration succeeded");
            Assert.That(output, Does.Contain("Reading client secret expiration for client"), "Output contains expected message");        
        }
    }
}
