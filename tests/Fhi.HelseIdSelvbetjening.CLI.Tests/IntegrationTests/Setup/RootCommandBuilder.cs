using Fhi.HelseIdSelvbetjening.CLI.Commands;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    internal class RootCommandBuilder
    {
        private string[] _args = Array.Empty<string>();

        public RootCommand RootCommand { get; private set; } = new RootCommand("Test Command");
        public string[] Args => _args;
        public IHelseIdSelvbetjeningService HelseIdSelvbetjeningServiceMock { get; private set; } = Substitute.For<IHelseIdSelvbetjeningService>();
        public ILogger LoggerMock { get; private set; } = Substitute.For<ILogger>();
        private readonly List<Action<IServiceCollection>> _registrations = new();


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

        public RootCommandBuilder WithSelvbetjeningService(IHelseIdSelvbetjeningService service)
        {
            _registrations.Add(services => services.AddSingleton(service));
            HelseIdSelvbetjeningServiceMock = service;
            return this;
        }
        public RootCommandBuilder WithLogger<T>(ILogger<T> service)
        {
            _registrations.Add(services => services.AddSingleton(service));
            LoggerMock = service;
            return this;
        }

        public RootCommandBuilder Build()
        {
            RootCommand = Program.BuildRootCommand(new CommandInput()
            {
                Args = _args,
                OverrideServices = services =>
                {
                    foreach (var registration in _registrations)
                    {
                        registration(services);
                    }
                }
            });
            return this;
        }
    }
}
