using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration
{
    internal class ReadClientSecretExpirationCommandBuilder(ReadClientSecretExpirationCommandHandler commandHandler) : ICommandBuilder
    {
        private readonly ReadClientSecretExpirationCommandHandler _commandHandler = commandHandler;

        public Command Build(IHost host)
        {
            var readExpirationCommand = new Command(
                ReadClientSecretExpirationParameterNames.CommandName,
                "Read client secret expiration date from HelseID")
            {
                TreatUnmatchedTokensAsErrors = true
            };

            var clientIdOption = readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.ClientId.Long,
                ReadClientSecretExpirationParameterNames.ClientId.Short,
                "Client ID for client to query",
                isRequired: true
            );

            var existingPrivateJwkPathOption = readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long,
                ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Short,
                "Path to the existing private key file",
                isRequired: false
            );

            var existingPrivateJwkOption = readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long,
                ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Short,
                "Existing private key value",
                isRequired: false
            );

            var authorityUrlOption = readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.AuthorityUrl.Long,
                ReadClientSecretExpirationParameterNames.AuthorityUrl.Short,
                "Authority url to query secret expiration with",
                isRequired: true
            );

            var baseAddressOption = readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.BaseAddress.Long,
                ReadClientSecretExpirationParameterNames.BaseAddress.Short,
                "Base Address url to query secret expiration with",
                isRequired: true
            );

            readExpirationCommand.SetHandler(async
            (
                clientId,
                existingPrivateJwkPath,
                existingPrivateJwk,
                authorityUrl,
                baseAddress
            ) =>
            {
                var parameters = new ReadClientSecretExpirationParameters
                {
                    ClientId = clientId,
                    ExistingPrivateJwkPath = existingPrivateJwkPath,
                    ExistingPrivateJwk = existingPrivateJwk,
                    AuthorityUrl = authorityUrl,
                    BaseAddress = baseAddress
                };
                await _commandHandler.ExecuteAsync(parameters);
            },
                clientIdOption,
                existingPrivateJwkPathOption,
                existingPrivateJwkOption,
                authorityUrlOption,
                baseAddressOption
            );

            return readExpirationCommand;
        }
    }
}
