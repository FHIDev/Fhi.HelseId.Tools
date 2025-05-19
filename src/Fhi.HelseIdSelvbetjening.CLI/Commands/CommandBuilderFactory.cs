using Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey;
using Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey;
using Microsoft.Extensions.DependencyInjection;

public partial class Program
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
                builder = new UpdateClientKeyCommandBuilder(input.Args);
            }
            else if (command == GenerateKeyParameterNames.CommandName)
            {
                builder = new GenerateKeyCommandBuilder();
            }

            return builder;
        }
    }
}
