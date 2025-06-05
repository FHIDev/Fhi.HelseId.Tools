using System.CommandLine;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey
{
    public class GenerateKeyCommandBuilder : BaseCommandBuilder
    {
        public override Action<IServiceCollection>? Services => services =>
        {
            services.AddTransient<IFileHandler, FileHandler>();
        };


        public override Command? Build(IHost host)
        {
            var generateKeyCommand = new Command(GenerateKeyParameterNames.CommandName, "Generate a new RSA key pair");

            var keyNameOption = new Option<string>([$"--{GenerateKeyParameterNames.KeyFileNamePrefix.Long}", $"--{GenerateKeyParameterNames.KeyFileNamePrefix.Short}"], "Prefix for the key file names");
            generateKeyCommand.AddOption(keyNameOption);

            var keyDirOption = new Option<string>([$"--{GenerateKeyParameterNames.KeyDirectory.Long}", $"--{GenerateKeyParameterNames.KeyDirectory.Short}"], "Directory to store the generated keys");
            generateKeyCommand.AddOption(keyDirOption);
            generateKeyCommand.TreatUnmatchedTokensAsErrors = true;

            var exceptionHandler = GetExceptionHandler(host);

            generateKeyCommand.SetHandler(exceptionHandler.WrapHandler(async (string? keyFileNamePrefix, string? keyDirectory) =>
            {
                var parameters = new GenerateKeyParameters
                {
                    KeyFileNamePrefix = keyFileNamePrefix,
                    KeyDirectory = keyDirectory
                };

                var logger = host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<KeyGeneratorService>>();
                var fileWriter = host.Services.GetRequiredService<IFileHandler>();
                var service = new KeyGeneratorService(parameters, fileWriter, logger);

                await service.ExecuteAsync();
            }), keyNameOption, keyDirOption);

            return generateKeyCommand;
        }
    }
}