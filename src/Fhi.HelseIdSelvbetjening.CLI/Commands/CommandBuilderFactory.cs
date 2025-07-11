using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey;
using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands
{
    public class CommandInput
    {
        public string[] Args { get; set; } = [];

        public Action<IServiceCollection>? OverrideServices { get; set; }
    }

    public class CommandBuilderFactory
    {
        public static ICommandBuilder? Create(CommandInput input)
        {
            var command = input.Args.FirstOrDefault()?.ToLowerInvariant();

            ICommandBuilder? builder = null;
            if (command == UpdateClientKeyParameterNames.CommandName)
            {
                builder = new UpdateClientKeyCommandBuilder();
            }
            else if (command == GenerateKeyParameterNames.CommandName)
            {
                builder = new GenerateKeyCommandBuilder();
            }
            else if (command == ReadClientSecretExpirationParameterNames.CommandName)
            {
                builder = new ReadClientSecretExpirationCommandBuilder();
            }

            return builder;
        }
    }
}