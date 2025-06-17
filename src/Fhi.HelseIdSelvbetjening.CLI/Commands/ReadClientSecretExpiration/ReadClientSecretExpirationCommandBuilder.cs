using System.CommandLine;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration
{
    public class ReadClientSecretExpirationCommandBuilder : ICommandBuilder
    {
        public Action<IServiceCollection>? Services => services =>
        {
            services.AddTransient<IFileHandler, FileHandler>();
            services.AddSelvbetjeningServices();
        };

        public Command Build(IHost host)
        {
            var readExpirationCommand = new Command(ReadClientSecretExpirationParameterNames.CommandName, "Read client secret expiration date from HelseID");

            var clientIdOption = new Option<string>(
                [$"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", $"-{ReadClientSecretExpirationParameterNames.ClientId.Short}"],
                description: "Client ID for client to query")
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
            readExpirationCommand.AddOption(clientIdOption);

            var existingPrivateJwkPathOption = new Option<string>(
                [$"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", $"-{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Short}"],
                "Path to the existing private key file");
            readExpirationCommand.AddOption(existingPrivateJwkPathOption);

            var existingPrivateJwkOption = new Option<string>(
                [$"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", $"-{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Short}"],
                "Existing private key value");
            readExpirationCommand.AddOption(existingPrivateJwkOption);

            readExpirationCommand.SetHandler(async (
                string clientId,
                string? existingPrivateJwkPath,
                string? existingPrivateJwk) =>
            {
                var logger = host.Services.GetRequiredService<ILogger<ClientSecretExpirationReaderService>>();

                try
                {
                    var environment = host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName;

                    var parameters = new ReadClientSecretExpirationParameters
                    {
                        ClientId = clientId,
                        ExistingPrivateJwkPath = existingPrivateJwkPath,
                        ExistingPrivateJwk = existingPrivateJwk
                    };

                    var fileHandler = host.Services.GetRequiredService<IFileHandler>();
                    var helseIdService = host.Services.GetRequiredService<IHelseIdSelvbetjeningService>();

                    var service = new ClientSecretExpirationReaderService(parameters, helseIdService, fileHandler, logger);

                    await service.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in ReadClientSecretExpiration command. Exception type: {ExceptionType}", ex.GetType().Name);
                }
            },
            clientIdOption, existingPrivateJwkPathOption, existingPrivateJwkOption);

            return readExpirationCommand;
        }
    }
}
