using System.CommandLine;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands
{
    public interface ICommandBuilder
    {
        Command Build(IHost host);
    }
}
