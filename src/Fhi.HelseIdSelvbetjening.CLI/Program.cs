using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Fhi.HelseId.Selvbetjening;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

/// <summary>
/// Executable Program for HelseId Selvbetjening CLI
/// </summary>
public partial class Program
{
    /// <summary>
    /// Main program
    /// </summary>
    /// <param name="args"></param>
    public static async Task<int> Main(string[] args)
    {
        // Setup logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create the host
        var host = CreateHostBuilder(args).Build();

        // Create root command
        var rootCommand = new RootCommand("HelseID self-service command line tool");

        // Configure generate-key command
        var generateKeyCommand = new Command("generatekey", "Generate a new RSA key pair");
        generateKeyCommand.AddAlias("generate-key");
        
        var keyNameOption = new Option<string>(
            new[] { "--keyFileNamePrefix", "-n" },
            "Prefix for the key file names");

        var keyDirOption = new Option<string>(
            new[] { "--keyDirectory", "-d" },
            "Directory to store the generated keys");
            
        generateKeyCommand.AddOption(keyNameOption);
        generateKeyCommand.AddOption(keyDirOption);
        generateKeyCommand.Handler = CommandHandler.Create<string?, string?>(
            (keyFileNamePrefix, keyDirectory) => HandleGenerateKeyCommand(keyFileNamePrefix, keyDirectory, host));
        rootCommand.AddCommand(generateKeyCommand);

        // Configure update-client-key command
        var updateClientKeyCommand = new Command("updateclientkey", "Update a client key in HelseID");
        updateClientKeyCommand.AddAlias("update-client-key");
        
        var clientIdOption = new Option<string>(
            new[] { "--clientId", "-c" },
            "Client ID to update")
        { IsRequired = true };
            
        var newPublicJwkPathOption = new Option<string>(
            new[] { "--newPublicJwkPath", "-np" },
            "Path to the new public key file");
            
        var existingPrivateJwkPathOption = new Option<string>(
            new[] { "--existingPrivateJwkPath", "-ep" },
            "Path to the existing private key file");
            
        var newPublicJwkOption = new Option<string>(
            new[] { "--newPublicJwk", "-n" },
            "New public key value");
            
        var existingPrivateJwkOption = new Option<string>(
            new[] { "--existingPrivateJwk", "-e" },
            "Existing private key value");
            
        updateClientKeyCommand.AddOption(clientIdOption);
        updateClientKeyCommand.AddOption(newPublicJwkPathOption);
        updateClientKeyCommand.AddOption(existingPrivateJwkPathOption);
        updateClientKeyCommand.AddOption(newPublicJwkOption);
        updateClientKeyCommand.AddOption(existingPrivateJwkOption);
        updateClientKeyCommand.Handler = CommandHandler.Create<string, string?, string?, string?, string?>(
            (clientId, newPublicJwkPath, existingPrivateJwkPath, newPublicJwk, existingPrivateJwk) => 
                HandleUpdateClientKeyCommand(clientId, newPublicJwkPath, existingPrivateJwkPath, newPublicJwk, existingPrivateJwk, host));
        rootCommand.AddCommand(updateClientKeyCommand);

        // Parse and invoke the command
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task<int> HandleGenerateKeyCommand(string? keyFileNamePrefix, string? keyDirectory, IHost host)
    {
        try
        {
            var parameters = new GenerateKeyParameters
            {
                KeyFileNamePrefix = keyFileNamePrefix,
                KeyDirectory = keyDirectory
            };

            var logger = host.Services.GetRequiredService<ILogger<KeyGeneratorService>>();
            var fileWriter = host.Services.GetRequiredService<IFileHandler>();
            var service = new KeyGeneratorService(parameters, fileWriter, logger);
            
            await service.ExecuteAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error generating key: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> HandleUpdateClientKeyCommand(
        string clientId, 
        string? newPublicJwkPath, 
        string? existingPrivateJwkPath,
        string? newPublicJwk,
        string? existingPrivateJwk,
        IHost host)
    {
        try
        {
            Console.WriteLine($"Environment: {host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName}");
            Console.WriteLine($"Update client in environment {host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName}? y/n");

            var input = Console.ReadLine();
            if (input?.Trim().ToLower() != "y")
            {
                Console.WriteLine("Operation cancelled.");
                return 0;
            }

            var parameters = new UpdateClientKeyParameters
            {
                ClientId = clientId,
                NewPublicJwkPath = newPublicJwkPath,
                ExistingPrivateJwkPath = existingPrivateJwkPath,
                ExisitingPrivateJwk = existingPrivateJwk,
                NewPublicJwk = newPublicJwk
            };

            var logger = host.Services.GetRequiredService<ILogger<ClientKeyUpdaterService>>();
            var fileHandler = host.Services.GetRequiredService<IFileHandler>();
            var helseIdService = host.Services.GetRequiredService<IHelseIdSelvbetjeningService>();
            
            var service = new ClientKeyUpdaterService(parameters, helseIdService, fileHandler, logger);
            
            await service.ExecuteAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating client key: {ex.Message}");
            return 1;
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Register common services
                services.AddTransient<IFileHandler, FileHandler>();
                
                // Register HelseId services for the updateclientkey command
                if (args.Length > 0 && args[0].ToLower() == "updateclientkey")
                {
                    services.Configure<SelvbetjeningConfiguration>(context.Configuration.GetSection("SelvbetjeningConfiguration"));
                    services.AddSelvbetjeningServices();
                }
            })
            .ConfigureLogging((context, config) =>
            {
                config.ClearProviders();
                config.AddSerilog(Log.Logger, dispose: true);
            });
}
