using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup
{
    internal class TestHostBuilder : CliHostBuilder
    {
        private readonly Action<IServiceCollection> _testOverrides;

        public TestHostBuilder(string[] args, Action<IServiceCollection> testOverrides)
            : base(args)
        {
            _testOverrides = testOverrides;
        }

        protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            base.ConfigureServices(context, services);
            _testOverrides?.Invoke(services);
        }
    }
}
