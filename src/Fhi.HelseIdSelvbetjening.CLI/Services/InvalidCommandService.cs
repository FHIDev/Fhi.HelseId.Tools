using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class InvalidCommandService : IHostedService
{
    private readonly ILogger<InvalidCommandService> _logger;

    public InvalidCommandService(ILogger<InvalidCommandService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogError("Invalid command! Use 'generatekey' or 'updateclientkey'.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
