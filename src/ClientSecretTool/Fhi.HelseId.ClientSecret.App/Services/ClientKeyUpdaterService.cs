using Fhi.HelseId.Selvbetjening;
using Fhi.HelseId.Selvbetjening.Models;
using Microsoft.Extensions.Hosting;
using static Program;

internal class ClientKeyUpdaterService : IHostedService
{
    private readonly UpdateClientKeyParameters _parameters;
    private readonly SelvbetjeningConfiguration _selvbetjeningConfiguration;
    private readonly IHelseIdSelvbetjeningService _helseIdSelvbetjeningService;

    public ClientKeyUpdaterService(UpdateClientKeyParameters updateClientKeyParameters,
        SelvbetjeningConfiguration selvbetjeningConfiguration,
        IHelseIdSelvbetjeningService helseIdSelvbetjeningService)
    {
        _parameters = updateClientKeyParameters;
        _selvbetjeningConfiguration = selvbetjeningConfiguration;
        _helseIdSelvbetjeningService = helseIdSelvbetjeningService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Update client Generation Parameters:");
        Console.WriteLine($"  - ClientId: {_parameters.ClientId}");
        Console.WriteLine($"  - KeyPath: {_parameters.PublicKeyPath}");
        Console.WriteLine($"  - HelseId authority: {_selvbetjeningConfiguration.Authority}");
        Console.WriteLine($"  - HelseId selvbetjening address: {_selvbetjeningConfiguration.BaseAddress}");
        Console.Write("Proceed with key generation? (y/n): ");

        string? input = Console.ReadLine();
        if (input?.Trim().ToLower() != "y")
        {
            Console.WriteLine("Operation cancelled.");
            return Task.CompletedTask;
        }

        //TODO: error handling
        var newKey = File.ReadAllText(_parameters.PublicKeyPath);
        Console.WriteLine($"New public key: {newKey} ");
        _helseIdSelvbetjeningService.UpdateClientSecret(new ClientConfiguration(_parameters.ClientId, _parameters.OldKey), newKey);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.FromCanceled(cancellationToken);
    }
}