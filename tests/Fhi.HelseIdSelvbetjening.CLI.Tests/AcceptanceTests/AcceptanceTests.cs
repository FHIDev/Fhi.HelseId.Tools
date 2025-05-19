using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;

namespace Fhi.HelseIdSelvbetjening.CLI.AcceptanceTests
{
    /// <summary>
    /// Manual acceptance tests for the CLI.
    /// </summary>
    public class AcceptanceTests
    {
        private readonly string testDirectory = Path.Combine(Environment.CurrentDirectory, "TestData");
        private readonly string keyName = "test";
        private readonly string keyFilePath;
        private readonly string publicKeyFilePath;

        public AcceptanceTests()
        {
            keyFilePath = Path.Combine(testDirectory, $"{keyName}_private.json");
            publicKeyFilePath = Path.Combine(testDirectory, $"{keyName}_public.json");
        }

        [Test]
        [Ignore("This test generates keys and should be run manually.")]
        public async Task GenerateKeys()
        {
            Directory.CreateDirectory(testDirectory);

            var args = new[] {
                "generatekey",
                GenerateKeyParameterNames.KeyFileNamePrefix.Long, keyName,
                GenerateKeyParameterNames.KeyDirectory.Long, testDirectory
            };

            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            await Program.Main(args);

            var output = stringWriter.ToString();
        }

        [Test]
        [Ignore("For manual testing")]
        public async Task InvallidCommand()
        {
            var args = new[] {
                "invalid",
            };

            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            await Program.Main(args);

            var output = stringWriter.ToString();
        }

        [Test]
        [Ignore("This test updates keys and should be run manually.")]
        public async Task UpdateClientKeysFromPath()
        {
            var newKeyPath = Path.Combine(testDirectory, "test_private.json");
            var oldKeyPath = Path.Combine(testDirectory, "oldkey.json");
            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName,
                "--env", "dev",
                UpdateClientKeyParameterNames.NewPublicJwkPath.Long, newKeyPath,
                UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long, oldKeyPath,
                UpdateClientKeyParameterNames.ClientId.Long, "88d474a8-07df-4dc4-abb0-6b759c2b99ec"
            };
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            await Program.Main(args);
        }
    }
}
