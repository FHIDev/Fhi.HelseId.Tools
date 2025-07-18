using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands
{
    internal class CommandBuilderRegister
    {
        private readonly IServiceProvider _provider;

        public CommandBuilderRegister(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IEnumerable<ICommandBuilder> Create()
        {
            return _provider.GetServices<ICommandBuilder>();
        }
    }

}