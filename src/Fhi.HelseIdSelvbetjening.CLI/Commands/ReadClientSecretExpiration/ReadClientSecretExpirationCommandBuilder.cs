using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;
using System.CommandLine.NamingConventionBinder;

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

            readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.ClientId.Long,
                ReadClientSecretExpirationParameterNames.ClientId.Short,
                "Client ID for client to query",
                isRequired: true
            );

            readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long,
                ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Short,
                "Path to the existing private key file",
                isRequired: false
            );

            readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long,
                ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Short,
                "Existing private key value",
                isRequired: false
            );

            readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.AuthorityUrl.Long,
                ReadClientSecretExpirationParameterNames.AuthorityUrl.Short,
                "Authority url to query secret expiration with",
                isRequired: true
            );

            readExpirationCommand.CreateStringOption(
                ReadClientSecretExpirationParameterNames.BaseAddress.Long,
                ReadClientSecretExpirationParameterNames.BaseAddress.Short,
                "Base Address url to query secret expiration with",
                isRequired: true
            );

            readExpirationCommand.Handler = CommandHandler.Create(async
            (
                string clientId,
                string existingPrivateJwkPath,
                string existingPrivateJwk,
                string authorityUrl,
                string baseAddress
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
                return await _commandHandler.ExecuteAsync(parameters);
            });

            return readExpirationCommand;
        }
    }
}
