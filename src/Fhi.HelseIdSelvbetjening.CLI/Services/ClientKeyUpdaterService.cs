using Fhi.HelseId.Selvbetjening.Services;
using Fhi.HelseId.Selvbetjening.Services.Models;
using Fhi.HelseIdSelvbetjening.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class ClientKeyUpdaterService : IHostedService
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Update client {@ClientId} ", _parameters.ClientId);

            string newKey = !string.IsNullOrEmpty(_parameters.NewKey) ? _parameters.NewKey :
            (!string.IsNullOrEmpty(_parameters.NewKeyPath) ? _fileHandler.ReadAllText(_parameters.NewKeyPath) : string.Empty);

            string? oldKey = !string.IsNullOrEmpty(_parameters.OldKey) ? _parameters.NewKey :
            (!string.IsNullOrEmpty(_parameters.OldKeyPath) ? _fileHandler.ReadAllText(_parameters.OldKeyPath) : string.Empty);


            if (!string.IsNullOrEmpty(newKey) && !string.IsNullOrEmpty(oldKey))
            {
                _logger.LogInformation($" - NewKey: {newKey} \n");
                _logger.LogInformation($" - OldKey: {oldKey} \n");
                _helseIdSelvbetjeningService.UpdateClientSecret(new ClientConfiguration(_parameters.ClientId, oldKey), newKey);
            }
            else
            {
                _logger.LogError($"Parameters empty. New key: {newKey} Old key: {oldKey}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}