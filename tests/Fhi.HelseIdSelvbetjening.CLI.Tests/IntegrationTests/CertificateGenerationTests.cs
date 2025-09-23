using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.CommandLine;
using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateCertificate;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    public class CertificateGenerationTests
    {
        [TestCase("--CertificateCommonName", "--CertificatePassword", "--CertificateDirectory")]
        [TestCase("-cn", "-pwd", "-dir")]
        public async Task GenerateCertificates(string commonName, string password, string directory)
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();
            var certName = "integration_test";
            var directoryPath = "c:\\temp";
            var certPassword = "TestPassword123!";
            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                $"{commonName}", certName,
                $"{directory}", directoryPath,
                $"{password}", certPassword
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
                Assert.That(fileHandlerMock.Files, Has.Count.EqualTo(3));
                Assert.That(exitCode, Is.EqualTo(0));

                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs, Does.Contain($"Private certificate saved: {Path.Combine(directoryPath, certName)}_private.pfx"));
                Assert.That(logs, Does.Contain($"Public certificate saved: {Path.Combine(directoryPath, certName)}_public.pem"));
                Assert.That(logs, Does.Contain($"Thumbprint saved: {Path.Combine(directoryPath, certName)}_thumbprint.txt"));
            }
        }

        [Test]
        public async Task GenerateKeys_PathIsEmpty_UseCurrentDirectory()
        {
            var fakeLogProvider = new FakeLoggerProvider();
            var fileHandlerMock = new FileHandlerMock();
            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                $"--{GenerateCertificateParameterNames.CertificateCommonName.Long}", "TestCert",
                $"--{GenerateCertificateParameterNames.CertificatePassword.Long}", "TestPassword123!"
            };
            
            var expectedPublicCertPath = Path.Combine(Environment.CurrentDirectory, "TestCert_public.pem");
            var expectedPrivateCertPath = Path.Combine(Environment.CurrentDirectory, "TestCert_private.pfx");
            var expectedThumbprintPath = Path.Combine(Environment.CurrentDirectory, "TestCert_thumbprint.txt");
            
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
                var logs = fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(exitCode, Is.EqualTo(0));

                Assert.That(logs, Does.Contain($"Private certificate saved: {expectedPrivateCertPath}"));
                Assert.That(logs, Does.Contain($"Public certificate saved: {expectedPublicCertPath}"));
                Assert.That(logs, Does.Contain($"Thumbprint saved: {expectedThumbprintPath}"));
            }
        }
        
        [Test]
        public async Task GenerateCertificates_MissingPassword_ThrowsError()
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();

            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                $"--{GenerateCertificateParameterNames.CertificateCommonName.Long}", "TestName",
            };

            var rootCommandBuilder = new RootCommandBuilder()
              .WithArgs(args)
              .WithFileHandler(fileHandlerMock)
              .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            using var errorWriter = new StringWriter();
            var originalError = Console.Error;
            Console.SetError(errorWriter);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);

            var commandLineBuilder = new CommandLineBuilder();
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            Console.SetError(originalError);

            using (Assert.EnterMultipleScope())
            {
                var errorOutput = errorWriter.ToString();
                Assert.That(exitCode, Is.Not.EqualTo(0));
                Assert.That(errorOutput, Does.Contain("Option '--CertificatePassword' is required"));
            }
        }
    }
}
