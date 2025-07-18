using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration
{
    internal class ReadClientSecretExpirationCommandBuilder : ICommandBuilder
    {
        private readonly ReadClientSecretExpirationCommandHandler _commandHandler;

        public ReadClientSecretExpirationCommandBuilder(ReadClientSecretExpirationCommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }
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

            readExpirationCommand.Handler = CommandHandler.Create(async (string clientId, string? existingPrivateJwkPath, string? existingPrivateJwk) =>
            {
                return await _commandHandler.ExecuteAsync(clientId, existingPrivateJwkPath, existingPrivateJwk);
            });

            return readExpirationCommand;
        }
    }
}
