using System.CommandLine;
using Fhi.HelseIdSelvbetjening;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public partial class Program
{
    public class UpdateClientKeyCommandBuilder : ICommandBuilder
    {
        public UpdateClientKeyCommandBuilder(string[] args)
        {
            Args = args;
        }
        public string[] Args { get; private set; }

        public Action<IServiceCollection>? Services => services =>
        {
            services.AddTransient<IFileHandler, FileHandler>();
            services.Configure<SelvbetjeningConfiguration>(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("SelvbetjeningConfiguration"));
            services.AddSelvbetjeningServices();
        };

        public Command Build(IHost host)
        {
            var updateClientKeyCommand = new Command(UpdateClientKeyParameterNames.CommandName, "Update a client key in HelseID");

            var clientIdOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.ClientId.Long}", $"-{UpdateClientKeyParameterNames.ClientId.Short}"],
                "Client ID to update")
            { IsRequired = true };
            var newPublicJwkPathOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", $"-{UpdateClientKeyParameterNames.NewPublicJwkPath.Short}"],
                "Path to the new public key file");
            var existingPrivateJwkPathOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", $"-{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Short}"],
                "Path to the existing private key file");
            var newPublicJwkOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.NewPublicJwk.Long}", $"-{UpdateClientKeyParameterNames.NewPublicJwk.Short}"],
                "New public key value");
            var existingPrivateJwkOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.ExistingPrivateJwk.Long}", $"-{UpdateClientKeyParameterNames.ExistingPrivateJwk.Short}"],
                "Existing private key value");

            updateClientKeyCommand.AddOption(clientIdOption);
            updateClientKeyCommand.AddOption(newPublicJwkPathOption);
            updateClientKeyCommand.AddOption(existingPrivateJwkPathOption);
            updateClientKeyCommand.AddOption(newPublicJwkOption);
            updateClientKeyCommand.AddOption(existingPrivateJwkOption);

            updateClientKeyCommand.SetHandler(async (
                string clientId,
                string? newPublicJwkPath,
                string? existingPrivateJwkPath,
                string? newPublicJwk,
                string? existingPrivateJwk) =>
            {
                try
                {
                    Console.WriteLine($"Environment: {host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName}");
                    Console.WriteLine($"Update client in environment {host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName}? y/n");

                    var input = Console.ReadLine();
                    if (input?.Trim().ToLower() != "y")
                    {
                        Console.WriteLine("Operation cancelled.");
                        return;
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
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"Error updating client key: {ex.Message}");
                }
            },
            clientIdOption, newPublicJwkPathOption, existingPrivateJwkPathOption, newPublicJwkOption, existingPrivateJwkOption);

            return updateClientKeyCommand;
        }
    }

}
