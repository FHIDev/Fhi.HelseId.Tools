using Fhi.HelseId.ClientSecret.App.Services;
using Fhi.HelseId.Selvbetjening;
using Fhi.HelseId.Selvbetjening.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

public partial class Program
{
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

        string? command = args.Length > 0 ? args[0] : null;
        if (command == "generatekey")
        {
            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new GenerateKeyParameters
                {
                    FileName = config["FileName"],
                    KeyPath = config["KeyPath"]
                };
            });

            services.AddHostedService<KeyGeneratorService>();
            services.AddHostedService<KeyGeneratorService>();

        }
        else if (command == "updateclientkey")
        {
            Console.WriteLine($"Environment: {context.HostingEnvironment.EnvironmentName}");
            Console.WriteLine($"Update client in environment {context.HostingEnvironment.EnvironmentName}? y/n");

            string? input = Console.ReadLine();
            if (input?.Trim().ToLower() != "y")
            {
                Console.WriteLine("Operation cancelled.");
                return;
            }

            var configuration = new ConfigurationBuilder()
                            .SetBasePath(Environment.CurrentDirectory)
                            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: false)
                            .AddCommandLine(args)
                            .Build();


            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new UpdateClientKeyParameters
                {
                    ClientId = config["ClientId"],
                    NewKeyPath = config["NewKeyPath"],
                    OldKeyPath = config["OldKeyPath"],
                    OldKey = config["OldKey"],
                    NewKey = config["NewKey"]
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
