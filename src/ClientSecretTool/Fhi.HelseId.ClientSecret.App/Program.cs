using Fhi.HelseId.ClientSecret.App.Services;
using Fhi.HelseId.Selvbetjening;
using Fhi.HelseId.Selvbetjening.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public class GenerateKeyParameters
    {
        public string? ClientId { get; set; }

        public string? KeyPath { get; set; }
    };

    public class UpdateClientKeyParameters
    {
        public string? ClientId { get; set; }
        public string? OldKey { get; set; }

        public string? PublicKeyPath { get; set; }
    };

    public static void Main(string[] args)
    {
        new HostBuilder()
           .ConfigureAppConfiguration(config =>
           {
               config.AddCommandLine(args);
           })
        .ConfigureServices((context, services) =>
        {
            ConfigureServices(args, context, services);
        })
        .Build()
        .Run();
    }

    internal static void ConfigureServices(string[] args, HostBuilderContext context, IServiceCollection services)
    {
        var config = context.Configuration;
        string? command = args.Length > 0 ? args[0] : null;

        if (command == "generatekey")
        {
            services.AddSingleton<GenerateKeyParameters>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new GenerateKeyParameters
                {
                    ClientId = config["ClientId"],
                    KeyPath = config["KeyPath"]
                };
            });

            services.AddHostedService<KeyGeneratorService>();

            // Add hosted services
            services.AddHostedService<KeyGeneratorService>();
        }
        else if (command == "updateclientkey")
        {
            services.AddSingleton<UpdateClientKeyParameters>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new UpdateClientKeyParameters
                {
                    ClientId = config["ClientId"],
                    PublicKeyPath = config["PublicKeyPath"],
                    OldKey = config["OldKey"]
                };
            });
            services.AddHostedService<ClientKeyUpdaterService>();
            services.Configure<ClientConfiguration>(context.Configuration.GetSection("SelvbetjeningConfiguration"));
            services.AddTransient<HelseIdSelvbetjeningService>();
        }
        else
        {
            services.AddSingleton<IHostedService, InvalidCommandService>();
        }
    }
}
