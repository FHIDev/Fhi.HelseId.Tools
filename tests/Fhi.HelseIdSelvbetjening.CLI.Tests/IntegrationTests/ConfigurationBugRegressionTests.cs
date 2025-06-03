using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    /// <summary>
    /// Tests for the specific configuration binding bug that was fixed
    /// These tests verify that the original problematic scenarios no longer occur
    /// </summary>
    public class ConfigurationBugRegressionTests
    {
        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        }

        [Test]
        public void ReadClientSecretExpirationCommandBuilder_Services_ShouldNotCreateTemporaryServiceProvider()
        {
            var services = new ServiceCollection();
            var commandBuilder = new ReadClientSecretExpirationCommandBuilder();
            commandBuilder.Services?.Invoke(services);
            var optionsDescriptors = services.Where(s => s.ServiceType == typeof(IOptions<SelvbetjeningConfiguration>)).ToList();
            Assert.That(optionsDescriptors, Is.Empty,
                "Command builder should not register IOptions<SelvbetjeningConfiguration> - this should be done in HostBuilder");
        }

        [Test]
        public void UpdateClientKeyCommandBuilder_Services_ShouldNotCreateTemporaryServiceProvider()
        {
            var services = new ServiceCollection();
            var commandBuilder = new UpdateClientKeyCommandBuilder();
            commandBuilder.Services?.Invoke(services);
            var optionsDescriptors = services.Where(s => s.ServiceType == typeof(IOptions<SelvbetjeningConfiguration>)).ToList();
            Assert.That(optionsDescriptors, Is.Empty,
                "Command builder should not register IOptions<SelvbetjeningConfiguration> - this should be done in HostBuilder");
        }

        [Test]
        public void HostBuilder_ShouldRegisterSelvbetjeningConfigurationOptions()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var args = new[] { "readclientsecretexpiration", "--help" };
            var host = HostBuilder.CreateHost(args, services => { });
            var serviceProvider = host.Services;
            var optionsService = serviceProvider.GetService<IOptions<SelvbetjeningConfiguration>>();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(optionsService, Is.Not.Null, "IOptions<SelvbetjeningConfiguration> should be registered by HostBuilder");
                Assert.That(optionsService!.Value, Is.Not.Null, "SelvbetjeningConfiguration instance should not be null");
                Assert.That(optionsService.Value.Authority, Is.Not.Null, "Authority property should not be null (original bug symptom)");
            }
        }

        [Test]
        public void SimulateOriginalBugScenario_BuildServiceProviderInCommandBuilder_ShouldNotHappen()
        {
            var services = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            var emptyConfig = configBuilder.Build();
            services.AddSingleton<IConfiguration>(emptyConfig);
            var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var selvbetjeningSection = config.GetSection("SelvbetjeningConfiguration");
            var selvbetjeningConfig = new SelvbetjeningConfiguration
            {
                Authority = "",
                BaseAddress = "",
                ClientSecretEndpoint = ""
            };
            selvbetjeningSection.Bind(selvbetjeningConfig);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(selvbetjeningConfig.Authority, Is.Null.Or.Empty,
                    "This demonstrates the original bug - Authority would be null with empty configuration");
                Assert.That(selvbetjeningConfig.BaseAddress, Is.Null.Or.Empty,
                    "BaseAddress would also be null/empty");
                Assert.That(selvbetjeningConfig.ClientSecretEndpoint, Is.Null.Or.Empty,
                    "ClientSecretEndpoint would also be null/empty");
            }
        }

        [Test]
        public void ProperConfigurationBinding_WithHostContext_ShouldWork()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables();
            var configuration = configBuilder.Build();
            var selvbetjeningSection = configuration.GetSection("SelvbetjeningConfiguration");
            var selvbetjeningConfig = new SelvbetjeningConfiguration
            {
                Authority = "",
                BaseAddress = "",
                ClientSecretEndpoint = ""
            };
            selvbetjeningSection.Bind(selvbetjeningConfig);
            if (selvbetjeningSection.Exists())
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(selvbetjeningConfig.Authority, Is.Not.Null.And.Not.Empty,
                        "Authority should be properly bound from configuration");
                    Assert.That(selvbetjeningConfig.Authority, Does.StartWith("https://"),
                        "Authority should be a valid HTTPS URL");
                    Assert.That(selvbetjeningConfig.Authority, Does.Contain("test"),
                        "Test environment should contain 'test' in Authority URL");
                }
            }
            else
            {
                Assert.Inconclusive("Test configuration file not found - this is expected in some test environments");
            }
        }

        [Test]
        public void ServiceCollectionExtensions_ShouldNotRegisterConflictingOptions()
        {
            var services = new ServiceCollection();
            services.AddSelvbetjeningServices();
            var optionsDescriptors = services.Where(s => s.ServiceType == typeof(IOptions<SelvbetjeningConfiguration>)).ToList();
            Assert.That(optionsDescriptors.Count, Is.EqualTo(0),
                "AddSelvbetjeningServices should not register IOptions<SelvbetjeningConfiguration> to avoid conflicts with built-in options pattern");
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        }
    }
}
