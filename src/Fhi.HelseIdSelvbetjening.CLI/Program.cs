using System.CommandLine;
using Fhi.HelseIdSelvbetjening.CLI;
using Fhi.HelseIdSelvbetjening.CLI.Commands;
using Fhi.HelseIdSelvbetjening.CLI.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

/// <summary>
/// Executable Program for HelseId Selvbetjening CLI
/// </summary>
public partial class Program
{
    /// <summary>
    /// Main program
    /// </summary>
    /// <param name="args"></param>
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var rootCommand = BuildRootCommand(new CommandInput() { Args = args });
        return await rootCommand.InvokeAsync(args);
    }    internal static RootCommand BuildRootCommand(CommandInput input)
    {
        //TODO: CommandbilderFactory should probably return a collaction of commands instead that will be added to rootcommand. Use Composite pattern?
        var commandBuilder = CommandBuilderFactory.Create(input);
        var host = HostBuilder.CreateHost(input.Args, services =>
        {
            // Register global exception handler at the application level
            services.AddTransient<GlobalExceptionHandler>();
            
            commandBuilder?.Services?.Invoke(services);
            input.OverrideServices?.Invoke(services);
        });

        var rootCommand = new RootCommand("HelseID self-service commands");
        var command = commandBuilder?.Build(host);
        rootCommand.AddCommand(command ?? CreateInvalidCommand());
        return rootCommand;
    }

    /// <summary>
    /// TODO: can also be a ICommandBuilder and added to the factory
    /// </summary>
    /// <returns></returns>
    private static Command CreateInvalidCommand()
    {
        var invalidCommand = new Command("invalid", "Invalid command.");

        // Use a simple synchronous handler since we can't access the DI container here
        invalidCommand.SetHandler(() =>
        {
            Console.Error.WriteLine("Invalid command.");
            Environment.ExitCode = 1;
        });

        return invalidCommand;
    }
}
