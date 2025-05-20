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
        public Action<IServiceCollection>? Services => services =>
        {
            services.AddTransient<IFileHandler, FileHandler>();
            services.Configure<SelvbetjeningConfiguration>(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("SelvbetjeningConfiguration"));
            services.AddSelvbetjeningServices();
        };

        public Command Build(IHost host)
        {
            //TODO: should have description on options
            var updateClientKeyCommand = new Command(UpdateClientKeyParameterNames.CommandName, "Update a client key in HelseID");

            //TODO: add validation of options (not sure why this is not working, see tests)
            var clientIdOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.ClientId.Long}", $"-{UpdateClientKeyParameterNames.ClientId.Short}"],
                description: "Client ID for client to update")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };
            clientIdOption.SetDefaultValue(null);
            clientIdOption.AddValidator(result =>
            {
                if (result.GetValueOrDefault<string>() == null)
                {
                    result.ErrorMessage = "Missing required parameter Client ID: --clientId/-c";
                }
            });
            updateClientKeyCommand.AddOption(clientIdOption);

            var newPublicJwkPathOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.NewPublicJwkPath.Long}", $"-{UpdateClientKeyParameterNames.NewPublicJwkPath.Short}"],
                "Path to the new public key file");
            updateClientKeyCommand.AddOption(newPublicJwkPathOption);

            var existingPrivateJwkPathOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long}", $"-{UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Short}"],
                "Path to the existing private key file");
            updateClientKeyCommand.AddOption(existingPrivateJwkPathOption);

            var newPublicJwkOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.NewPublicJwk.Long}", $"-{UpdateClientKeyParameterNames.NewPublicJwk.Short}"],
                "New public key value");
            updateClientKeyCommand.AddOption(newPublicJwkOption);

            var existingPrivateJwkOption = new Option<string>(
                [$"--{UpdateClientKeyParameterNames.ExistingPrivateJwk.Long}", $"-{UpdateClientKeyParameterNames.ExistingPrivateJwk.Short}"],
                "Existing private key value");
            updateClientKeyCommand.AddOption(existingPrivateJwkOption);

            var yesOption = new Option<bool>(
                [$"--{UpdateClientKeyParameterNames.YesOption.Long}", $"--{UpdateClientKeyParameterNames.YesOption.Short}"],
                "Automatically confirm update without prompting");
            updateClientKeyCommand.AddOption(yesOption);

            updateClientKeyCommand.SetHandler(async (
                string clientId,
                string? newPublicJwkPath,
                string? existingPrivateJwkPath,
                string? newPublicJwk,
                string? existingPrivateJwk,
                bool yes) =>
            {
                try
                {
                    //TODO: write to log and not console?
                    var environment = host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName;
                    Console.WriteLine($"Environment: {environment}");
                    if (!yes)
                    {
                        Console.WriteLine($"Update client in environment {environment}? y/n");
                        var input = Console.ReadLine();
                        if (input?.Trim().ToLower() != "y")
                        {
                            Console.WriteLine("Operation cancelled.");
                            return;
                        }
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

                    //TODO: Do we need ClientUpdateService? potentially move logic to that service. Now logic is in two places
                    var service = new ClientKeyUpdaterService(parameters, helseIdService, fileHandler, logger);

                    await service.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"Error updating client key: {ex.Message}");
                }
            },
            clientIdOption, newPublicJwkPathOption, existingPrivateJwkPathOption, newPublicJwkOption, existingPrivateJwkOption, yesOption);

            return updateClientKeyCommand;
        }
    }

}
