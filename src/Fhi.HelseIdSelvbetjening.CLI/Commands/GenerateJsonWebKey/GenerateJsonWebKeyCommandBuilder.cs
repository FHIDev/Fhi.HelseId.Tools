using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateJsonWebKey
{
    internal class GenerateJsonWebKeyCommandBuilder(JsonWebKeyGeneratorHandler commandHandler) : ICommandBuilder
    {
        private readonly JsonWebKeyGeneratorHandler _commandHandler = commandHandler;

        public Command Build(IHost host)
        {
            var generateJsonWebKeyCommand = new Command(
                GenerateJsonWebKeyParameterNames.CommandName,
                "Generate a new RSA key pair")
            {
                TreatUnmatchedTokensAsErrors = true
            };

            var keyNameOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Long,
                GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Short,
                "Prefix for the key file names",
                isRequired: true);

            var keyDirOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.KeyDirectory.Long,
                GenerateJsonWebKeyParameterNames.KeyDirectory.Short,
                "Directory to store the generated keys",
                isRequired: false);

            generateJsonWebKeyCommand.SetHandler(async (keyFileNamePrefix, keyDirectory) =>
            {
                var parameters = new GenerateJsonWebKeyParameters
                {
                    KeyFileNamePrefix = keyFileNamePrefix,
                    KeyDirectory = keyDirectory
                };
                await _commandHandler.Execute(parameters);
            },
            keyNameOption, keyDirOption);

            return generateJsonWebKeyCommand;
        }
    }
}