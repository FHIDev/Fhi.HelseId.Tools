using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;

namespace Fhi.HelseId.ClientSecret.App.Tests.AcceptanceTests
{
    // TODO: Investigate why the HttpClient is not working from the test host and move it to a separate test project
    public class AcceptanceTest
    {
        [Test]
        public async Task GenerateKeys()
        {
            // Arrange
            var testDirectory = Path.Combine(Environment.CurrentDirectory, "TestData");
            Directory.CreateDirectory(testDirectory); // Ensure directory exists
            
            var keyName = "test";
            var keyFilePath = Path.Combine(testDirectory, $"{keyName}_private.json");
            var publicKeyFilePath = Path.Combine(testDirectory, $"{keyName}_public.json");
            
            // Cleanup any existing files from previous test runs
            if (File.Exists(keyFilePath)) File.Delete(keyFilePath);
            if (File.Exists(publicKeyFilePath)) File.Delete(publicKeyFilePath);
            
            var args = new[] { "generatekey", "--keyFileNamePrefix", keyName, "--keyDirectory", testDirectory };

            // Redirect console output for assertion
            var originalOutput = Console.Out;
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                // Act
                int exitCode = await Program.Main(args);

                var output = stringWriter.ToString();
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(exitCode, Is.EqualTo(0));
                    Assert.That(File.Exists(keyFilePath), Is.True, "Private key file was not created");
                    Assert.That(File.Exists(publicKeyFilePath), Is.True, "Public key file was not created");
                    Assert.That(output, Contains.Substring("Private key saved"));
                    Assert.That(output, Contains.Substring("Public key saved"));
                }
            }
            finally
            {
                // Restore console output
                Console.SetOut(originalOutput);
            }
        }

        [Test]
        public async Task UpdateClientKeysFromPath()
        {
            // Arrange
            var testDirectory = Path.Combine(Environment.CurrentDirectory, "TestData");
            Directory.CreateDirectory(testDirectory); // Ensure directory exists
            
            // Setup test key files
            var newKeyName = "test_public";
            var oldKeyName = "oldkey";
            var newKeyPath = Path.Combine(testDirectory, $"{newKeyName}.json");
            var oldKeyPath = Path.Combine(testDirectory, $"{oldKeyName}.json");
            
            // Create mock key files with basic content
            var mockPublicJwk = "{\r\n  \"e\": \"AQAB\",\r\n  \"kid\": \"test-kid\",\r\n  \"kty\": \"RSA\",\r\n  \"n\": \"test-key-data\"\r\n}";
            var mockPrivateJwk = "{\r\n  \"d\": \"test-private-data\",\r\n  \"e\": \"AQAB\",\r\n  \"kid\": \"test-kid\",\r\n  \"kty\": \"RSA\",\r\n  \"n\": \"test-key-data\"\r\n}";
            
            File.WriteAllText(newKeyPath, mockPublicJwk);
            File.WriteAllText(oldKeyPath, mockPrivateJwk);
            
            var args = new[]
            {
                "updateclientkey",
                "--newPublicJwkPath", newKeyPath,
                "--existingPrivateJwkPath", oldKeyPath,
                "--clientId", "88d474a8-07df-4dc4-abb0-6b759c2b99ec"
            };

            // Set environment to development
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "development");

            // Redirect console output and input for interactive confirmation
            var originalOutput = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            var originalInput = Console.In;
            using var input = new StringReader("y\n");
            Console.SetIn(input);

            try
            {
                // Act
                int exitCode = await Program.Main(args);
                var output = stringWriter.ToString().Trim();
                
                // Assert
                Assert.That(exitCode, Is.EqualTo(0), "Command should exit with code 0");
                Assert.That(output, Contains.Substring("Environment: development"), "Output should show environment information");
                Assert.That(output, Contains.Substring("Update client 88d474a8-07df-4dc4-abb0-6b759c2b99ec"), "Output should show client update process");
            }
            finally
            {
                // Restore console input/output
                Console.SetOut(originalOutput);
                Console.SetIn(originalInput);
                
                // Clean up test files
                if (File.Exists(newKeyPath)) File.Delete(newKeyPath);
                if (File.Exists(oldKeyPath)) File.Delete(oldKeyPath);
            }
        }

        [Test]
        public async Task UpdateClientKeysFromParameters()
        {
            // Arrange
            var args = new[]
            {
                "updateclientkey",
                "--newPublicJwk", "{\r\n  \"e\": \"AQAB\",\r\n  \"key_ops\": [],\r\n  \"kid\": \"t416Ss3pDr0WNShuU0Q1543RVSXeY9Vbqc1wHIx_kF8\",\r\n  \"kty\": \"RSA\",\r\n  \"n\": \"tPRvKBIs0Wcugola1Xzb3mAMkIg3tN8q8vRfjxaglvrEJ1b4ITazcHpSMvqwt2dLj7f6bw5ti-mL8_vOW--3tE3DL7ZTHvF-pazD2WV_aQUv5k5UKdOOmVhDJyJXFq7CMn-NoVBgQvMl84X8oZkIXSb1MdgeevvaUdn02aYgN9joQZFQcJLgEm1D8GPp3z4oloxBGX6c2mcqndusmda1iZckkeC_hN-9a0Uqq5azPO4WkbsBz_hPIl8qZbVWoyDaVcLAvS9k42SNox5TLdik9C0Kr6P2FWm9pIq-apNfTogYzGcvtMRp3BjBZ33OYl0rMj0g-D_oHYHyESJjlLwH0Q\",\r\n  \"oth\": [],\r\n  \"x5c\": []\r\n}",
                "--existingPrivateJwk", "{\"d\":\"Q4x8XiZ3JKn0-ijW-H9plfw7QF4VLK43jHxYtPJvX6GcBuEk_rMedziQuqbBCZrK6aWVspnYS6dQtj33Z2TtSkXu2gy_1xR2nR8h9XeZ6h6QRbL9bj1Qxrk70ry7bXz5WIjyyuPmY73aPw9OFrZ_NDeUQjiEofzTHkr86ZIVjAmNLarVufG9P2V6fz14wwHc3aLBVgUt7Rxx5sFOQR30zYGpd1BH-xK6ykA6n6BdaIc4luWw_SkmVowwO4toScj07qoAYTUR4IFQHYt7sQZNufFG89nB-v_Er0a2tRvtME2NnU_4rn4ea1yyGFlYH_6Amtb8u4-TAeOESjrMw9ylBkvb6vIvtqT0lQdBJJEPI_Hx-655ElvO4zT48HBS6oVZHCARN17d7pQWrnxiSusYEdM9RwJET57ieVayo-baQe3NOvj2Y5V2H034cWCJt_DTh7ye9RXD4gtMnHDQ-tgV6ztwW8GkGvbJzXUnkqGXUvKqjeJAnOc2Ahoxpc-9cnMnW2DrwPnI0f9Jsq0n3hQyqwnnyimIeZn32WVe2Q4XC7d_VB21E8oDZhdeUlxuTZX-foTrYB3xvDKB6tLCaaMbfpzvUsSfSYqbAXQfqhQWosyt7w-ZIYJOY05fWspR3mlpo5IMGkaDp8clvz51f8zdMfSYFTml4e_zjoduvlz2wyE\"}",
                "--clientId", "88d474a8-07df-4dc4-abb0-6b759c2b99ec"
            };

            // Set environment to development
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "development");

            // Redirect console output and input for interactive confirmation
            var originalOutput = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            var originalInput = Console.In;
            using var input = new StringReader("y\n");
            Console.SetIn(input);

            try
            {
                // Act
                int exitCode = await Program.Main(args);
                var output = stringWriter.ToString().Trim();

                // Assert
                Assert.That(exitCode, Is.EqualTo(0), "Command should exit with code 0");
                Assert.That(output, Contains.Substring("Environment: development"), "Output should show environment information");
                Assert.That(output, Contains.Substring("Update client 88d474a8-07df-4dc4-abb0-6b759c2b99ec"), "Output should show client update process");
            }
            finally
            {
                // Restore console input/output
                Console.SetOut(originalOutput);
                Console.SetIn(originalInput);
            }
        }
    }
}
