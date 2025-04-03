using Microsoft.Extensions.Logging;

internal class InvalidCommandService
{
    private readonly ILogger<InvalidCommandService> _logger;

    public InvalidCommandService(ILogger<InvalidCommandService> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync()
    {
        _logger.LogError("Invalid command! Use 'generatekey' or 'updateclientkey'.");
        return Task.CompletedTask;
    }
}
