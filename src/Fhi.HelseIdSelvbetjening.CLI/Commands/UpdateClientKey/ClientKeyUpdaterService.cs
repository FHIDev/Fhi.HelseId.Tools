using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey
{
    internal class ClientKeyUpdaterService
    {
        private readonly UpdateClientKeyParameters _parameters;
        private readonly IHelseIdSelvbetjeningService _helseIdSelvbetjeningService;
        private readonly IFileHandler _fileHandler;
        private readonly ILogger<ClientKeyUpdaterService> _logger;

        public ClientKeyUpdaterService(UpdateClientKeyParameters updateClientKeyParameters,
            IHelseIdSelvbetjeningService helseIdSelvbetjeningService,
            IFileHandler fileHandler,
            ILogger<ClientKeyUpdaterService> logger)
        {
            _parameters = updateClientKeyParameters;
            _helseIdSelvbetjeningService = helseIdSelvbetjeningService;
            _fileHandler = fileHandler;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Update client {@ClientId} ", _parameters.ClientId);

                var newKey = !string.IsNullOrEmpty(_parameters.NewPublicJwk) ? _parameters.NewPublicJwk :
                !string.IsNullOrEmpty(_parameters.NewPublicJwkPath) ? _fileHandler.ReadAllText(_parameters.NewPublicJwkPath) : string.Empty;

                var oldKey = !string.IsNullOrEmpty(_parameters.ExisitingPrivateJwk) ? _parameters.NewPublicJwk :
                !string.IsNullOrEmpty(_parameters.ExistingPrivateJwkPath) ? _fileHandler.ReadAllText(_parameters.ExistingPrivateJwkPath) : string.Empty;

                //TODO: handled by the options? Set required parameters?
                if (!string.IsNullOrEmpty(newKey) && !string.IsNullOrEmpty(oldKey))
                {
                    _logger.LogInformation("NewKey: {newKey}", newKey);
                    _logger.LogInformation("OldKey: {oldKey}", oldKey);
                    await _helseIdSelvbetjeningService.UpdateClientSecret(new ClientConfiguration(_parameters.ClientId, oldKey), newKey);
                }
                else
                {
                    _logger.LogError("Parameters empty. New key: {newKey} Old key: {oldKey}", newKey, oldKey);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}