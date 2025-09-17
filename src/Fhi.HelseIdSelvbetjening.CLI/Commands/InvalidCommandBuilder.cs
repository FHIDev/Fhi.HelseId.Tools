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

            invalidCommand.SetAction((ParseResult parseResult, CancellationToken cancellationToken) =>
            {
                Console.Error.WriteLine("Invalid command.");
                return Task.FromResult(1);
            });

            return invalidCommand;
        }
    }
}