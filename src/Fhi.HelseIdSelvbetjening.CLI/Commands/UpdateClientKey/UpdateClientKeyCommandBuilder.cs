using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey
{
    internal class UpdateClientKeyCommandBuilder : ICommandBuilder
    {
        private readonly ClientKeyUpdaterCommandHandler _commandHandler;

        public UpdateClientKeyCommandBuilder(ClientKeyUpdaterCommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }
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

            updateClientKeyCommand.Handler = CommandHandler.Create(async (
                string clientId,
                string newPublicJwkPath,
                string existingPrivateJwkPath,
                string existingPrivateJwk,
                string newPublicJwk,
                bool yes) =>
            {
                var parameters = new UpdateClientKeyParameters
                {
                    ClientId = clientId,
                    NewPublicJwkPath = newPublicJwkPath,
                    ExistingPrivateJwkPath = existingPrivateJwkPath,
                    ExistingPrivateJwk = existingPrivateJwk,
                    NewPublicJwk = newPublicJwk,
                    Yes = yes
                };
                return await _commandHandler.ExecuteAsync(parameters);
            });

            return updateClientKeyCommand;
        }
    }

}
