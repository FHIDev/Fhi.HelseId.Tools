using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.CommandLine;
namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public class KeyGenerationTests
    {
        [TestCase("--KeyFileNamePrefix", "--KeyDirectory")]
        [TestCase("-n", "-d")]
        public async Task GenerateKeys(string filePrefix, string directory)
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();
            var prefixName = "integration_test";
            var directoryPath = "c:\\temp";
            var args = new[]
            {
                GenerateKeyParameterNames.CommandName,
                $"{filePrefix}", prefixName,
                $"{directory}", directoryPath
            };
            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(args)
                .WithFileHandler(fileHandlerMock)
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            int exitCode = await rootCommandBuilder.Build().InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(fileHandlerMock.Files, Has.Count.EqualTo(2));
                Assert.That(exitCode, Is.EqualTo(0));

                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain($"Private key saved: {Path.Combine(directoryPath, prefixName)}_private.json"));
                Assert.That(logs!, Does.Contain($"Public key saved: {Path.Combine(directoryPath, prefixName)}_public.json"));
            }
        }

        [Test]
        [Ignore("todo")]
        public async Task GenerateKeys_InvalidParameterAsync()
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();
            var args = new[]
            {
                GenerateKeyParameterNames.CommandName,
                "--invalidparameter", "integration_test"
            };
            var rootCommandBuilder = new RootCommandBuilder()
               .WithArgs(args)
               .WithFileHandler(fileHandlerMock)
               .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            int exitCode = await rootCommandBuilder.Build().InvokeAsync(args);
            using (Assert.EnterMultipleScope())
            {
                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();

                Assert.That(exitCode, Is.Not.EqualTo(0));
                //TODO:Figure out what error message should be
                Assert.That(logs, Does.Contain("Unrecognized option '--invalidparameter'").IgnoreCase
                    .Or.Contain("Unknown option").IgnoreCase
                    .Or.Contain("is not a recognized option").IgnoreCase);
                Assert.That(logs!, Does.Contain(@"Private key saved: c:\temp\integration_test_private.json"));
                Assert.That(logs!, Does.Contain(@"Public key saved: c:\temp\integration_test_public.json"));
            }
        }

        [Test]
        public async Task GenerateKeys_PathIsNotEmpty_AddKeysToSpecifiedPath()
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();
            var args = new[]
            {
                GenerateKeyParameterNames.CommandName,
                $"--{GenerateKeyParameterNames.KeyFileNamePrefix.Long}", "TestClient",
                $"--{GenerateKeyParameterNames.KeyDirectory.Long}", "C:\\TestKeys"
            };
            var rootCommandBuilder = new RootCommandBuilder()
              .WithArgs(args)
              .WithFileHandler(fileHandlerMock)
              .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            int exitCode = await rootCommandBuilder.Build().InvokeAsync(args);

            using (Assert.EnterMultipleScope())
            {
                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                var expectedPublicKeyPath = Path.Combine("C:\\TestKeys", "TestClient_public.json");
                var expectedPrivateKeyPath = Path.Combine("C:\\TestKeys", "TestClient_private.json");
                Assert.That(exitCode, Is.EqualTo(0));

                Assert.That(logs!, Does.Contain($"Private key saved: {expectedPrivateKeyPath}"));
                Assert.That(logs!, Does.Contain($"Public key saved: {expectedPublicKeyPath}"));
            }
        }

        [Test]
        public async Task GenerateKeys_PathIsEmpty_UseCurrentDirectory()
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var fileHandlerMock = new FileHandlerMock();
            var args = new[]
            {
                GenerateKeyParameterNames.CommandName,
                $"--{GenerateKeyParameterNames.KeyFileNamePrefix.Long}", "TestClient"
            };
            var rootCommandBuilder = new RootCommandBuilder()
              .WithArgs(args)
              .WithFileHandler(fileHandlerMock)
              .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            int exitCode = await rootCommandBuilder.Build().InvokeAsync(args);

            using (Assert.EnterMultipleScope())
            {
                var expectedPublicKeyPath = Path.Combine(Environment.CurrentDirectory, "TestClient_public.json");
                var expectedPrivateKeyPath = Path.Combine(Environment.CurrentDirectory, "TestClient_private.json");
                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(exitCode, Is.EqualTo(0));

                Assert.That(logs!, Does.Contain($"Private key saved: {expectedPrivateKeyPath}"));
                Assert.That(logs!, Does.Contain($"Public key saved: {expectedPublicKeyPath}"));
            }
        }
    }
}
