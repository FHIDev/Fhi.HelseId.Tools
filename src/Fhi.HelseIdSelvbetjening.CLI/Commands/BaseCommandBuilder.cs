using Fhi.HelseIdSelvbetjening.CLI.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands
{
    /// <summary>
    /// Base class for command builders that provides common functionality like exception handling
    /// </summary>
    public abstract class BaseCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Override this to register command-specific services
        /// </summary>
        public virtual Action<IServiceCollection>? Services => null;

        /// <summary>
        /// Build the command - must be implemented by derived classes
        /// </summary>
        /// <param name="host">The host containing services</param>
        /// <returns>The built command</returns>
        public abstract System.CommandLine.Command? Build(IHost host);

        /// <summary>
        /// Gets the global exception handler from the host services
        /// </summary>
        /// <param name="host">The host containing services</param>
        /// <returns>The global exception handler instance</returns>
        protected GlobalExceptionHandler GetExceptionHandler(IHost host)
        {
            return host.Services.GetRequiredService<GlobalExceptionHandler>();
        }
    }
}
