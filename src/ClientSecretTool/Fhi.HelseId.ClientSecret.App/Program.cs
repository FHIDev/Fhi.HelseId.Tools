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
        public string? FileName { get; set; }

        public string? KeyPath { get; set; }
    };

    public class UpdateClientKeyParameters
    {
        public string? ClientId { get; set; }
        public string? OldKeyPath { get; set; }
        public string? OldKey { get; set; }

        public string? NewKeyPath { get; set; }
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
        .ConfigureLogging((context, logging) =>
        {
            //logging.Services.AddLogging();
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
                    FileName = config["FileName"],
                    KeyPath = config["KeyPath"]
                };
            });

            services.AddHostedService<KeyGeneratorService>();

            // Add hosted services
            services.AddHostedService<KeyGeneratorService>();
        }
        else if (command == "updateclientkey")
        {
            var envArg = args.FirstOrDefault(a => a.StartsWith("--env="));
            var environment = envArg?.Split("=")[1] ?? "prod";

            Console.WriteLine($"Using environment: {environment}");

            var configuration = new ConfigurationBuilder()
                            .SetBasePath(Environment.CurrentDirectory)
                            .AddJsonFile($"appsettings.{environment}.json", optional: false)
                            .Build();

            services.AddSingleton<UpdateClientKeyParameters>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new UpdateClientKeyParameters
                {
                    ClientId = config["ClientId"],
                    NewKeyPath = config["NewKeyPath"],
                    OldKeyPath = config["OldKeyPath"],
                    OldKey = config["OldKey"]
                };
            });

            services.AddHostedService<ClientKeyUpdaterService>();
            var selvbetjeningConfig = new SelvbetjeningConfiguration();
            var section = context.Configuration.GetSection("SelvbetjeningConfiguration");
            section.Bind(selvbetjeningConfig);
            services.Configure<SelvbetjeningConfiguration>(section);

            Console.WriteLine($"  - HelseId authority: {selvbetjeningConfig.Authority}");
            Console.WriteLine($"  - HelseId selvbetjening address: {selvbetjeningConfig.BaseAddress}");
            Console.Write("Proceed with key generation? (y/n): ");

            services.AddTransient<IHelseIdSelvbetjeningService, HelseIdSelvbetjeningService>();
        }
        else
        {
            services.AddSingleton<IHostedService, InvalidCommandService>();
        }
    }
}
