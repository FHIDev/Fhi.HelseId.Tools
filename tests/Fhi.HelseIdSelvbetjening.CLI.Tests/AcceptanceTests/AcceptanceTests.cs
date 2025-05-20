using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;

namespace Fhi.HelseIdSelvbetjening.CLI.AcceptanceTests
{
    /// <summary>
    /// Manual acceptance tests for the CLI. These tests should be runned against test environment.
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
        [Ignore("This test generates keys and update client with new keys should be run manually.")]
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
            Assert.That(exitCodeGenerateKeys, Is.EqualTo(0), "Generation of keys successed");

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
            Assert.That(exitCodeUpdateClient, Is.EqualTo(0), "Update of client keys successed");
        }
    }
}
