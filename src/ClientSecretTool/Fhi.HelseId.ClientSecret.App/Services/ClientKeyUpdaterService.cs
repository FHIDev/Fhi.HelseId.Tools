using Fhi.HelseId.Selvbetjening;
using Fhi.HelseId.Selvbetjening.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Program;

internal class ClientKeyUpdaterService : IHostedService
{
    private readonly UpdateClientKeyParameters _parameters;
    private readonly IHelseIdSelvbetjeningService _helseIdSelvbetjeningService;
    private readonly ILogger<ClientKeyUpdaterService> _logger;

    public ClientKeyUpdaterService(UpdateClientKeyParameters updateClientKeyParameters,
        IHelseIdSelvbetjeningService helseIdSelvbetjeningService,
        ILogger<ClientKeyUpdaterService> logger)
    {
        _parameters = updateClientKeyParameters;
        _helseIdSelvbetjeningService = helseIdSelvbetjeningService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Update client Generation Parameters:");
        Console.WriteLine($"  - ClientId: {_parameters.ClientId}");
        Console.WriteLine($"  - NewKeyPath: {_parameters.NewKeyPath}");
        Console.WriteLine($"  - OldKeyPath: {_parameters.OldKeyPath}");

        string? input = Console.ReadLine();
        if (input?.Trim().ToLower() != "y")
        {
            Console.WriteLine("Operation cancelled.");
            return Task.CompletedTask;
        }

        //TODO: error handling
        var newKey = File.ReadAllText(_parameters.NewKeyPath);
        var oldKey = _parameters.OldKey;//File.ReadAllText(_parameters.OldKeyPath);

        Console.WriteLine($"New public key: {newKey} ");
        _helseIdSelvbetjeningService.UpdateClientSecret(new ClientConfiguration(_parameters.ClientId, oldKey), newKey);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}