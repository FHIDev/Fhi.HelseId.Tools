using Fhi.HelseIdSelvbetjening.Business;
using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey
{
    internal class ClientKeyUpdaterCommandHandler(
        IHostEnvironment hostEnvironment,
        IHelseIdSelvbetjeningService helseIdSelvbetjeningService,
        IFileHandler fileHandler,
        ILogger<ClientKeyUpdaterCommandHandler> logger)
    {
        private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
        private readonly IHelseIdSelvbetjeningService _helseIdSelvbetjeningService = helseIdSelvbetjeningService;
        private readonly IFileHandler _fileHandler = fileHandler;
        private readonly ILogger<ClientKeyUpdaterCommandHandler> _logger = logger;

        public async Task<int> ExecuteAsync(UpdateClientKeyParameters parameters)
        {
            try
            {
                _logger.LogInformation("Update client {@ClientId}", parameters.ClientId);

                var environment = _hostEnvironment.EnvironmentName;
                _logger.LogInformation("Environment: {environment}", environment);
                if (!parameters.Yes)
                {
                    _logger.LogInformation("Update client in environment {environment}? y/n", environment);
                    var input = Console.ReadLine();
                    if (input?.Trim().ToLower() != "y")
                    {
                        _logger.LogInformation("Operation cancelled.");
                        return 0;
                    }
                }

                var newKey = KeyResolutionExtensions.ResolveKey(
                    parameters.NewPublicJwk,
                    parameters.NewPublicJwkPath,
                    "New key",
                    _logger,
                    _fileHandler);

                var oldKey = KeyResolutionExtensions.ResolveKey(
                    parameters.ExistingPrivateJwk,
                    parameters.ExistingPrivateJwkPath,
                    "Old key",
                    _logger,
                    _fileHandler);

                bool newKeyExists = !string.IsNullOrEmpty(newKey);
                bool oldKeyExists = !string.IsNullOrEmpty(oldKey);

                if (!newKeyExists || !oldKeyExists)
                {
                    _logger.LogError("One or more parameters empty.");
                    _logger.LogInformation("New key found: {newKeyExists} Old key found: {oldKeyExists}", newKeyExists, oldKeyExists);
                    return 1;
                }

                var result = await _helseIdSelvbetjeningService.UpdateClientSecret(new ClientConfiguration(
                parameters.ClientId, oldKey),
                parameters.AuthorityUrl, parameters.BaseAddress, newKey);
                _logger.LogInformation("Result http status: {resultStatus}", result.HttpStatus.ToString());
                _logger.LogInformation("Result http message: {resultMessage}", result.Message);

                return 0;
            }
            catch (Exception e)
            {
                _logger.LogError("Error message: {e.Message}", e.Message);
                return 1;
            }
        }
    }
}