using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;
using System.CommandLine.NamingConventionBinder;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey
{
    internal class UpdateClientKeyCommandBuilder(ClientKeyUpdaterCommandHandler commandHandler) : ICommandBuilder
    {
        private readonly ClientKeyUpdaterCommandHandler _commandHandler = commandHandler;

        public Command Build(IHost host)
        {
            var updateClientKeyCommand = new Command(
                UpdateClientKeyParameterNames.CommandName,
                "Update a client key in HelseID")
            {
                TreatUnmatchedTokensAsErrors = true
            };

            updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.ClientId.Long,
                UpdateClientKeyParameterNames.ClientId.Short,
                "Client ID for client to update",
                isRequired: true
            );

            updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.NewPublicJwkPath.Long,
                UpdateClientKeyParameterNames.NewPublicJwkPath.Short,
                "Path to the new public key file",
                isRequired: false
            );

            updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long,
                UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Short,
                "Path to the existing private key file",
                isRequired: false
            );

            updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.NewPublicJwk.Long,
                UpdateClientKeyParameterNames.NewPublicJwk.Short,
                "New public key value",
                isRequired: false
            );

            updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.ExistingPrivateJwk.Long,
                UpdateClientKeyParameterNames.ExistingPrivateJwk.Short,
                "Existing private key value",
                isRequired: false
            );

            updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.AuthorityUrl.Long,
                UpdateClientKeyParameterNames.AuthorityUrl.Short,
                "Authority url to update secret with",
                isRequired: true
            );

            updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.BaseAddress.Long,
                UpdateClientKeyParameterNames.BaseAddress.Short,
                "Base Address url to update secret with",
                isRequired: true
            );

            updateClientKeyCommand.CreateBoolOption(
                UpdateClientKeyParameterNames.YesOption.Long,
                UpdateClientKeyParameterNames.YesOption.Short,
                "Automatically confirm update without prompting user",
                defaultValue: false
            );

            updateClientKeyCommand.Handler = CommandHandler.Create(async (
                string clientId,
                string newPublicJwkPath,
                string existingPrivateJwkPath,
                string existingPrivateJwk,
                string newPublicJwk,
                string authorityUrl,
                string baseAddress,
                bool yes) =>
            {
                var parameters = new UpdateClientKeyParameters
                {
                    ClientId = clientId,
                    NewPublicJwkPath = newPublicJwkPath,
                    ExistingPrivateJwkPath = existingPrivateJwkPath,
                    ExistingPrivateJwk = existingPrivateJwk,
                    NewPublicJwk = newPublicJwk,
                    AuthorityUrl = authorityUrl,
                    BaseAddress = baseAddress,
                    Yes = yes
                };
                return await _commandHandler.ExecuteAsync(parameters);
            });


            return updateClientKeyCommand;
        }
    }
}
