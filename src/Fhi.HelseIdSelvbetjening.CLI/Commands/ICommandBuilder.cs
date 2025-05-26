using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands;
public interface ICommandBuilder
{
    public Action<IServiceCollection>? Services { get; }
    Command? Build(IHost host);
}
