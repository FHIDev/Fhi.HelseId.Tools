using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public partial class Program
{
    public interface ICommandBuilder
    {
        public Action<IServiceCollection>? Services { get; }
        Command? Build(IHost host);
    }
}
