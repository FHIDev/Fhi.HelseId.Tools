using Fhi.HelseId.Selvbetjening;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

/// <summary>
/// Executable Program for HelseId Serlvbetjening CLI
/// </summary>
public partial class Program
{
    /// <summary>
    /// Main program
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(config =>
           {
               config.AddCommandLine(args);
           })
        .ConfigureServices((context, services) =>
        {
            ConfigureServices(args, context, services);
        })
        .ConfigureLogging((context, config) =>
        {
            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger();

            config.ClearProviders();
            config.AddSerilog(Log.Logger, dispose: true);
        })
        .Build();

        host.Run();
    }

    internal static void ConfigureServices(string[] args, HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IFileHandler, FileHandler>();

        var command = args.Length > 0 ? args[0] : null;
        if (command == "generatekey")
        {
            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new GenerateKeyParameters
                {
                    KeyFileNamePrefix = config[nameof(GenerateKeyParameters.KeyFileNamePrefix)],
                    KeyDirectory = config[nameof(GenerateKeyParameters.KeyDirectory)]
                };
            });

            services.AddHostedService<KeyGeneratorService>();

        }
        else if (command == "updateclientkey")
        {
            Console.WriteLine($"Environment: {context.HostingEnvironment.EnvironmentName}");
            Console.WriteLine($"Update client in environment {context.HostingEnvironment.EnvironmentName}? y/n");

            var input = Console.ReadLine();
            if (input?.Trim().ToLower() != "y")
            {
                Console.WriteLine("Operation cancelled.");
                return;
            }

            var configuration = new ConfigurationBuilder()
                            .SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: false)
                            .AddCommandLine(args)
                            .Build();

            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var clientId = config["ClientId"];
                return new UpdateClientKeyParameters
                {
                    ClientId = config[nameof(UpdateClientKeyParameters.ClientId)] ?? string.Empty,
                    NewPublicJwkPath = config[nameof(UpdateClientKeyParameters.NewPublicJwkPath)],
                    ExistingPrivateJwkPath = config[nameof(UpdateClientKeyParameters.ExistingPrivateJwkPath)],
                    ExisitingPrivateJwk = config[nameof(UpdateClientKeyParameters.ExisitingPrivateJwk)],
                    NewPublicJwk = config[nameof(UpdateClientKeyParameters.NewPublicJwk)]
                };
            });
            services.AddHostedService<ClientKeyUpdaterService>();

            services.Configure<SelvbetjeningConfiguration>(configuration.GetSection("SelvbetjeningConfiguration"));
            services.AddSelvbetjeningServices();
        }
        else
        {
            services.AddSingleton<IHostedService, InvalidCommandService>();
        }
    }
}
