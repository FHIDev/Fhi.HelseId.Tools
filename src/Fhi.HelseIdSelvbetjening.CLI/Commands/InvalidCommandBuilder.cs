using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands
{
    internal class InvalidCommandBuilder : ICommandBuilder
    {
        public Action<IServiceCollection>? Services => throw new NotImplementedException();

        public Command Build(IHost host)
        {
            var invalidCommand = new Command("invalid", "Invalid command.");
            invalidCommand.SetHandler(() =>
            {
                Console.Error.WriteLine("Invalid command.");
            });

            return invalidCommand;
        }
    }
}
