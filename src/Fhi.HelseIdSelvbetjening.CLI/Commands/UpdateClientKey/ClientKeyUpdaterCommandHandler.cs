using Fhi.HelseIdSelvbetjening.Business;
using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey
{
    internal class ClientKeyUpdaterCommandHandler
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IHelseIdSelvbetjeningService _helseIdSelvbetjeningService;
        private readonly IFileHandler _fileHandler;
        private readonly ILogger<ClientKeyUpdaterCommandHandler> _logger;

        public ClientKeyUpdaterCommandHandler(
            IHostEnvironment hostEnvironment,
            IHelseIdSelvbetjeningService helseIdSelvbetjeningService,
            IFileHandler fileHandler,
            ILogger<ClientKeyUpdaterCommandHandler> logger)
        {
            _hostEnvironment = hostEnvironment;
            _helseIdSelvbetjeningService = helseIdSelvbetjeningService;
            _fileHandler = fileHandler;
            _logger = logger;
        }

        public async Task<int> ExecuteAsync(UpdateClientKeyParameters parameters)
        {
            try
            {
                var environment = _hostEnvironment;
                _logger.LogInformation($"Environment: {environment}");
                if (!parameters.Yes)
                {
                    _logger.LogInformation($"Update client in environment {environment}? y/n");
                    var input = Console.ReadLine();
                    if (input?.Trim().ToLower() != "y")
                    {
                        _logger.LogInformation("Operation cancelled.");
                        return 0;
                    }
                }

                if (string.IsNullOrEmpty(parameters.AuthorityUrl) || string.IsNullOrEmpty(parameters.BaseAddress))
                {
                    _logger.LogError("Parameters empty. Authority: {parameters.Authority} BaseAddress: {parameters.BaseAddress}", parameters.AuthorityUrl, parameters.BaseAddress);
                    return 1;
                }

                _logger.LogInformation("Update client {@ClientId}", parameters.ClientId);

                var newKey = !string.IsNullOrEmpty(parameters.NewPublicJwk) ? parameters.NewPublicJwk :
                !string.IsNullOrEmpty(parameters.NewPublicJwkPath) ? _fileHandler.ReadAllText(parameters.NewPublicJwkPath) : string.Empty;

                var oldKey = !string.IsNullOrEmpty(parameters.ExistingPrivateJwk) ? parameters.NewPublicJwk :
                !string.IsNullOrEmpty(parameters.ExistingPrivateJwkPath) ? _fileHandler.ReadAllText(parameters.ExistingPrivateJwkPath) : string.Empty;

                //TODO: handled by the options? Set required parameters?
                if (!string.IsNullOrEmpty(newKey) && !string.IsNullOrEmpty(oldKey))
                {
                    var result = await _helseIdSelvbetjeningService.UpdateClientSecret(new ClientConfiguration(parameters.ClientId, oldKey), parameters.AuthorityUrl, parameters.BaseAddress, newKey);
                    _logger.LogInformation(result.HttpStatus.ToString());
                    _logger.LogInformation(result.Message);
                }
                else
                {
                    _logger.LogError("Parameters empty. New key: {newKey} Old key: {oldKey}", newKey, oldKey);
                    return 1;
                }

                return 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return 1;
            }
        }
    }
}