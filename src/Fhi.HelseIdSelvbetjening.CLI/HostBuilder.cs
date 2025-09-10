using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.CLI.Commands;
using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateCertificate;
using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey;
using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Fhi.HelseIdSelvbetjening.CLI
{
    internal class CliHostBuilder
    {
        protected readonly string[] _args;

        public CliHostBuilder(string[] args)
        {
            _args = args;
        }

        public IHost BuildHost()
        {
            return Host.CreateDefaultBuilder(_args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    var basePath = AppContext.BaseDirectory;
                    config.SetBasePath(basePath);
                    config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddCommandLine(_args);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog(Log.Logger, dispose: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<SelvbetjeningConfiguration>(context.Configuration.GetSection(nameof(SelvbetjeningConfiguration)));
                    services.AddTransient<IFileHandler, FileHandler>();
                    services.AddSelvbetjeningServices();

                    services.AddTransient<ICommandBuilder, UpdateClientKeyCommandBuilder>();
                    services.AddTransient<ClientKeyUpdaterCommandHandler>();
                    services.AddTransient<ICommandBuilder, GenerateKeyCommandBuilder>();
                    services.AddTransient<ICommandBuilder, GenerateCertificateCommandBuilder>();
                    services.AddTransient<GenerateCertificateCommandHandler>();
                    services.AddTransient<ICommandBuilder, ReadClientSecretExpirationCommandBuilder>();
                    services.AddTransient<ReadClientSecretExpirationCommandHandler>();
                    services.AddTransient<ICommandBuilder, InvalidCommandBuilder>();

                    ConfigureServices(context, services);
                })
                .Build();
        }

        protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
        }
    }
}