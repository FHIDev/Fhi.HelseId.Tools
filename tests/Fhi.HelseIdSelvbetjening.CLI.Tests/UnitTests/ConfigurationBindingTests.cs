using Fhi.HelseIdSelvbetjening.Business.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fhi.HelseIdSelvbetjening.CLI.UnitTests
{
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
            var hostBuilder = new CliHostBuilder(args);
            var host = hostBuilder.BuildHost();
            var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Value, Is.Not.Null, "SelvbetjeningConfiguration should be bound");
                Assert.That(config.Value.Authority, Is.Not.Null.And.Not.Empty, "Authority should not be null or empty");
                Assert.That(config.Value.Authority, Does.Contain("https://helseid-sts.test.nhn.no"), "Authority should be a valid HTTPS URL");
                Assert.That(config.Value.BaseAddress, Does.Contain("https://api.selvbetjening.test.nhn.no"), "BaseAddress should not be null or empty");
            }
        }

        [Test]
        public void HostBuilder_WithProductionEnvironment_ShouldLoadProductionConfiguration()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
            var args = new[] { "readclientsecretexpiration", "--help" };
            var hostBuilder = new CliHostBuilder(args);
            var host = hostBuilder.BuildHost();
            var config = host.Services.GetRequiredService<IOptions<SelvbetjeningConfiguration>>();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Value, Is.Not.Null, "SelvbetjeningConfiguration should be bound");
                Assert.That(config.Value.Authority, Does.Contain("https://helseid-sts.nhn.no"), "Authority should be a valid HTTPS URL");
                Assert.That(config.Value.BaseAddress, Does.Contain("https://api.selvbetjening.nhn.no"), "BaseAddress should not be null or empty");
            }
        }

        [Test]
        public void Configuration_WithInvalidEnvironment_ShouldFallbackToDefault()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "NonExistentEnvironment"); var args = new[] { "readclientsecretexpiration", "--help" };
            Assert.DoesNotThrow(() =>
            {
                var hostBuilder = new CliHostBuilder(args);
                var host = hostBuilder.BuildHost();
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
