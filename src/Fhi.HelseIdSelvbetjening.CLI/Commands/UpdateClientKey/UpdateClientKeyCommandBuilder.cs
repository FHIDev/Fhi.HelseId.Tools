using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;

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

            var clientIdOption = updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.ClientId.Long,
                UpdateClientKeyParameterNames.ClientId.Short,
                "Client ID for client to update",
                isRequired: true);

            var newPublicJwkPathOption = updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.NewPublicJwkPath.Long,
                UpdateClientKeyParameterNames.NewPublicJwkPath.Short,
                "Path to the new public key file",
                isRequired: false);

            var existingPrivateJwkPathOption = updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Long,
                UpdateClientKeyParameterNames.ExistingPrivateJwkPath.Short,
                "Path to the existing private key file",
                isRequired: false);

            var newPublicJwkOption = updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.NewPublicJwk.Long,
                UpdateClientKeyParameterNames.NewPublicJwk.Short,
                "New public key value",
                isRequired: false);

            var existingPrivateJwkOption = updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.ExistingPrivateJwk.Long,
                UpdateClientKeyParameterNames.ExistingPrivateJwk.Short,
                "Existing private key value",
                isRequired: false);

            var authorityUrlOption = updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.AuthorityUrl.Long,
                UpdateClientKeyParameterNames.AuthorityUrl.Short,
                "Authority url to update secret with",
                isRequired: true);

            var baseAddressOption = updateClientKeyCommand.CreateStringOption(
                UpdateClientKeyParameterNames.BaseAddress.Long,
                UpdateClientKeyParameterNames.BaseAddress.Short,
                "Base Address url to update secret with",
                isRequired: true);

            var yesOption = updateClientKeyCommand.CreateBoolOption(
                UpdateClientKeyParameterNames.YesOption.Long,
                UpdateClientKeyParameterNames.YesOption.Short,
                "Automatically confirm update without prompting user");

            updateClientKeyCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
            {
                var parameters = new UpdateClientKeyParameters
                {
                    // TODO: fix "may be null"
                    ClientId = parseResult.GetValue(clientIdOption)!,
                    NewPublicJwkPath = parseResult.GetValue(newPublicJwkPathOption),
                    ExistingPrivateJwkPath = parseResult.GetValue(existingPrivateJwkPathOption),
                    NewPublicJwk = parseResult.GetValue(newPublicJwkOption),
                    ExistingPrivateJwk = parseResult.GetValue(existingPrivateJwkOption),
                    AuthorityUrl = parseResult.GetValue(authorityUrlOption)!,
                    BaseAddress = parseResult.GetValue(baseAddressOption)!,
                    Yes = parseResult.GetValue(yesOption)
                };

                return await _commandHandler.ExecuteAsync(parameters);
            });

            return updateClientKeyCommand;
        }
    }
}
