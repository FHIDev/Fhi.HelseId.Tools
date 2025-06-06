using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Fhi.HelseIdSelvbetjening.CLI
{
    internal static class HostBuilder
    {
        internal static IHost CreateHost(string[] args, Action<IServiceCollection> configureServices)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddCommandLine(args);
                })
                .ConfigureLogging((context, config) =>
                {
                    config.ClearProviders();
                    config.AddSerilog(Log.Logger, dispose: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Configure SelvbetjeningConfiguration from the properly loaded configuration
                    services.Configure<Fhi.HelseIdSelvbetjening.Services.Models.SelvbetjeningConfiguration>(
                        context.Configuration.GetSection("SelvbetjeningConfiguration"));

                    configureServices(services);
                })
                .Build();
        }
    }

}
