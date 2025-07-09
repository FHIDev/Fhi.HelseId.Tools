using Fhi.HelseIdSelvbetjening.CLI.Commands;
using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    /// <summary>
    /// Tests for configuration binding scenarios that were previously broken
    /// </summary>
    public class ConfigurationBindingTests
    {
        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        }

        [Test]
        public void HostBuilder_WithTestEnvironment_ShouldLoadTestConfiguration()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var args = new[] { "readclientsecretexpiration", "--help" };
            var host = HostBuilder.CreateHost(args, services => { });
            var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Value, Is.Not.Null, "SelvbetjeningConfiguration should be bound");
                Assert.That(config.Value.Authority, Is.Not.Null.And.Not.Empty, "Authority should not be null or empty");
                Assert.That(config.Value.Authority, Does.StartWith("https://"), "Authority should be a valid HTTPS URL");
                Assert.That(config.Value.Authority, Does.Contain("test"), "Test environment should contain 'test' in Authority URL");
                Assert.That(config.Value.BaseAddress, Is.Not.Null.And.Not.Empty, "BaseAddress should not be null or empty");
            }
        }

        [Test]
        public void HostBuilder_WithProductionEnvironment_ShouldLoadProductionConfiguration()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
            var args = new[] { "readclientsecretexpiration", "--help" };
            var host = HostBuilder.CreateHost(args, services => { });
            var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Value, Is.Not.Null, "SelvbetjeningConfiguration should be bound");
                Assert.That(config.Value.Authority, Is.Not.Null.And.Not.Empty, "Authority should not be null or empty");
                Assert.That(config.Value.Authority, Does.StartWith("https://"), "Authority should be a valid HTTPS URL");
                Assert.That(config.Value.Authority, Does.Not.Contain("test"), "Production environment should not contain 'test' in Authority URL");
                Assert.That(config.Value.BaseAddress, Is.Not.Null.And.Not.Empty, "BaseAddress should not be null or empty");
            }
        }

        [Test]
        public void ReadClientSecretExpirationCommand_ShouldHaveProperlyBoundConfiguration()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var args = new[]
            {
                ReadClientSecretExpirationParameterNames.CommandName,
                "--help"
            };
            var rootCommand = Program.BuildRootCommand(new CommandInput() { Args = args });
            var host = HostBuilder.CreateHost(args, services =>
            {
                var commandBuilder = new ReadClientSecretExpirationCommandBuilder();
                commandBuilder.Services?.Invoke(services);
            });
            var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Value, Is.Not.Null, "SelvbetjeningConfiguration should be bound");
                Assert.That(config.Value.Authority, Is.Not.Null, "Authority should not be null (this was the original bug)");
                Assert.That(config.Value.Authority, Does.StartWith("https://"), "Authority should be a valid HTTPS URL");
            }
        }

        [Test]
        public void UpdateClientKeyCommand_ShouldHaveProperlyBoundConfiguration()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName,
                "--help"
            };
            var rootCommand = Program.BuildRootCommand(new CommandInput() { Args = args });
            var host = HostBuilder.CreateHost(args, services =>
            {
                var commandBuilder = new UpdateClientKeyCommandBuilder();
                commandBuilder.Services?.Invoke(services);
            });
            var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Value, Is.Not.Null, "SelvbetjeningConfiguration should be bound");
                Assert.That(config.Value.Authority, Is.Not.Null, "Authority should not be null (this was the original bug)");
                Assert.That(config.Value.Authority, Does.StartWith("https://"), "Authority should be a valid HTTPS URL");
                Assert.That(config.Value.Authority, Does.Contain("test"), "Test environment should contain 'test' in Authority URL");
            }
        }

        [Test]
        public void HelseIdSelvbetjeningService_WithProperConfiguration_ShouldNotThrowNullReference()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var args = new[] { "readclientsecretexpiration", "--help" };
            var host = HostBuilder.CreateHost(args, services =>
            {
                services.AddSelvbetjeningServices();
                services.AddTransient<IFileHandler, FileHandlerMock>();
                services.AddLogging();
            });
            using (Assert.EnterMultipleScope())
            {
                Assert.DoesNotThrow(() =>
                {
                    var service = host.Services.GetRequiredService<IHelseIdSelvbetjeningService>();
                }, "Creating HelseIdSelvbetjeningService should not throw with properly bound configuration");
                var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
                Assert.That(config.Value.Authority, Is.Not.Null, "Authority should be properly bound from configuration");
            }
        }

        [Test]
        public void SelvbetjeningConfiguration_ShouldNotConflictWithBuiltInOptionsPattern()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var args = new[] { "readclientsecretexpiration", "--help" };
            var host = HostBuilder.CreateHost(args, services =>
            {
                services.AddSelvbetjeningServices();
            });
            using (Assert.EnterMultipleScope())
            {
                var options1 = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
                var options2 = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
                Assert.That(options1, Is.Not.Null, "First IOptions<SelvbetjeningConfiguration> should resolve");
                Assert.That(options2, Is.Not.Null, "Second IOptions<SelvbetjeningConfiguration> should resolve");
                Assert.That(options1.Value.Authority, Is.EqualTo(options2.Value.Authority), "Both instances should have same configuration values");
                Assert.That(options1.Value.Authority, Does.StartWith("https://"), "Authority should be a valid HTTPS URL");
                Assert.That(options1.Value.Authority, Does.Contain("test"), "Test environment should contain 'test' in Authority URL");
            }
        }

        [Test]
        public void CommandBuilder_WithMockedServices_ShouldStillBindConfiguration()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var fileHandlerMock = new FileHandlerMock();
            var loggerMock = Substitute.For<ILogger<ClientKeyUpdaterService>>();
            var helseIdServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
            var args = new[]
            {
                UpdateClientKeyParameterNames.CommandName,
                "--help"
            };
            var rootCommand = Program.BuildRootCommand(new CommandInput()
            {
                Args = args,
                OverrideServices = services =>
                {
                    services.AddSingleton<IFileHandler>(fileHandlerMock);
                    services.AddSingleton(loggerMock);
                    services.AddSingleton(helseIdServiceMock);
                }
            });
            var commandBuilder = new UpdateClientKeyCommandBuilder();
            var host = HostBuilder.CreateHost(args, services =>
            {
                commandBuilder.Services?.Invoke(services);
                services.AddSingleton<IFileHandler>(fileHandlerMock);
                services.AddSingleton(loggerMock);
                services.AddSingleton(helseIdServiceMock);
            });
            var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Value, Is.Not.Null, "Configuration should be bound even with mocked services");
                Assert.That(config.Value.Authority, Is.Not.Null, "Authority should not be null");
                Assert.That(config.Value.Authority, Does.StartWith("https://"), "Authority should be a valid HTTPS URL");
                Assert.That(config.Value.Authority, Does.Contain("test"), "Test environment should contain 'test' in Authority URL");
            }
        }

        [Test]
        public void Configuration_WithInvalidEnvironment_ShouldFallbackToDefault()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "NonExistentEnvironment"); var args = new[] { "readclientsecretexpiration", "--help" };
            Assert.DoesNotThrow(() =>
            {
                var host = HostBuilder.CreateHost(args, services => { });
                var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
                Assert.That(config.Value, Is.Not.Null, "Configuration should still bind even for invalid environment");
            }, "Should not throw for invalid environment name");
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        }
    }
}
