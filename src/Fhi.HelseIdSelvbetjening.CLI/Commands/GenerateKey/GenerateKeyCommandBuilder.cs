using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey
{
    internal class GenerateKeyCommandBuilder(KeyGeneratorHandler commandHandler) : ICommandBuilder
    {
        private readonly KeyGeneratorHandler _commandHandler = commandHandler;

        public Command Build(IHost host)
        {
            var generateKeyCommand = new Command(
                GenerateKeyParameterNames.CommandName,
                "Generate a new RSA key pair")
            {
                TreatUnmatchedTokensAsErrors = true
            };

            var keyNameOption = generateKeyCommand.CreateStringOption(
                GenerateKeyParameterNames.KeyFileNamePrefix.Long,
                GenerateKeyParameterNames.KeyFileNamePrefix.Short,
                "Prefix for the key file names",
                isRequired: true);

            var keyDirOption = generateKeyCommand.CreateStringOption(
                GenerateKeyParameterNames.KeyDirectory.Long,
                GenerateKeyParameterNames.KeyDirectory.Short,
                "Directory to store the generated keys",
                isRequired: true);

            generateKeyCommand.SetHandler(async (keyFileNamePrefix, keyDirectory) =>
            {
                var parameters = new GenerateKeyParameters
                {
                    KeyFileNamePrefix = keyFileNamePrefix,
                    KeyDirectory = keyDirectory
                };
                await _commandHandler.Execute(parameters);
            },
            keyNameOption, keyDirOption);

            return generateKeyCommand;
        }
    }
}