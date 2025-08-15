using System.CommandLine;
using System.CommandLine.Parsing;
using Fhi.HelseIdSelvbetjening.CLI;
using Fhi.HelseIdSelvbetjening.CLI.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

/// <summary>
/// Executable Program for HelseId Selvbetjening CLI
/// </summary>
public partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        var command = BuildRootCommand(new CliHostBuilder(args));
        return await command.InvokeAsync(args);
    }

    internal static RootCommand BuildRootCommand(CliHostBuilder hostBuilder)
    {
        var host = hostBuilder.BuildHost();
        var commandBuilders = host.Services.GetServices<ICommandBuilder>();

        var rootCommand = new RootCommand("HelseID self-service commands");
        foreach (var builder in commandBuilders)
        {
            var command = builder.Build(host);
            rootCommand.AddCommand(command);
        }

        return rootCommand;
    }
}
