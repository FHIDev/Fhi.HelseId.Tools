using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateJsonWebKey;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.CommandLine;
namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public class KeyGenerationTests
    {
        [TestCase("--KeyFileNamePrefix", "--KeyDirectory", "")]
        [TestCase("--KeyFileNamePrefix", "--KeyDirectory", "--KeyCustomKid")]
        [TestCase("-n", "-d", "")]
        [TestCase("-n", "-d", "-k")]
        public async Task GenerateJsonWebKeys(string prefixOption, string directoryPathOption, string? customKidOption)
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();

            var prefixName = "integration_test";
            var directoryPath = "c:\\temp";

            var args = new List<string>
            {
                GenerateJsonWebKeyParameterNames.CommandName,
                $"{prefixOption}", prefixName,
                $"{directoryPathOption}", directoryPath
            };

            if (!string.IsNullOrWhiteSpace(customKidOption))
            {
                args.Add($"{customKidOption}");
                args.Add("customKidTest");
            }

            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(args.ToArray())
                .WithFileHandler(fileHandlerMock)
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(fileHandlerMock.Files, Has.Count.EqualTo(2));
                Assert.That(exitCode, Is.EqualTo(0));

                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain($"Private key saved: {Path.Combine(directoryPath, prefixName)}_private.json"));
                Assert.That(logs!, Does.Contain($"Public key saved: {Path.Combine(directoryPath, prefixName)}_public.json"));
            }
        }

        /**
        // TODO: needs upgrade to version beta5 of system.commandLine
        [Test]
        [Ignore("todo")]
        public async Task GenerateJsonWebKeys_InvalidParameterAsync()
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();
            var args = new[]
            {
                GenerateJsonWebKeyParameterNames.CommandName,
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
        }*/

        [Test]
        public async Task GenerateJsonWebKeys_PathIsEmpty_UseCurrentDirectory()
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var fileHandlerMock = new FileHandlerMock();
            var args = new[]
            {
                GenerateJsonWebKeyParameterNames.CommandName,
                $"--{GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Long}", "TestClient",
                $"--KeyCustomKid", "TESSTSTST"
            };
            var rootCommandBuilder = new RootCommandBuilder()
              .WithArgs(args)
              .WithFileHandler(fileHandlerMock)
              .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

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
