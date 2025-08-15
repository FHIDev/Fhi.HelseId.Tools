using Fhi.HelseIdSelvbetjening.Business;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup
{
    internal class RootCommandBuilder
    {
        private string[] _args = [];

        public string[] Args => _args;
        private readonly List<Action<IServiceCollection>> _registrations = [];


        public RootCommandBuilder WithArgs(string[] args)
        {
            _args = args;
            return this;
        }

        public RootCommandBuilder WithFileHandler(IFileHandler fileHandlerMock)
        {
            _registrations.Add(services => services.AddSingleton(fileHandlerMock));
            return this;
        }

        public RootCommandBuilder WithLoggerProvider(ILoggerProvider logProvider, LogLevel logLevel)
        {
            var factory = LoggerFactory.Create(loggerBuilder =>
            {
                loggerBuilder.AddProvider(logProvider);
                loggerBuilder.SetMinimumLevel(logLevel);
            });

            _registrations.Add(services =>
            {
                services.AddLogging();
                services.AddSingleton(factory);
                services.AddSingleton(factory.CreateLogger("general logs"));
            });
            return this;
        }

        public RootCommandBuilder WithSelvbetjeningService(IHelseIdSelvbetjeningService service)
        {
            _registrations.Add(services => services.AddSingleton(service));
            return this;
        }

        public RootCommand Build()
        {
            var testHost = new TestHostBuilder(_args, services =>
            {
                foreach (var registration in _registrations)
                {
                    registration(services);
                }
            });

            return Program.BuildRootCommand(testHost);
        }
    }
}
