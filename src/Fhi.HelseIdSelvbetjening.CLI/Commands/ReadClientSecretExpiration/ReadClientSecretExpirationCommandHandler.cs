using Fhi.HelseIdSelvbetjening.Business;
using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration
{
    internal class ReadClientSecretExpirationCommandHandler(
        ILogger<ReadClientSecretExpirationCommandHandler> logger,
        IHelseIdSelvbetjeningService helseIdService,
        IFileHandler fileHandler)
    {
        private readonly ILogger<ReadClientSecretExpirationCommandHandler> _logger = logger;
        private readonly IHelseIdSelvbetjeningService _helseIdService = helseIdService;
        private readonly IFileHandler _fileHandler = fileHandler;

        public async Task<int> ExecuteAsync(string clientId, string authority, string baseAddress, string? existingPrivateJwkPath, string? existingPrivateJwk)
        {
            try
            {
                using (_logger.BeginScope("ClientId: {ClientId}", clientId))
                {
                    var privateKey = !string.IsNullOrWhiteSpace(existingPrivateJwk) ? existingPrivateJwk :
                        !string.IsNullOrWhiteSpace(existingPrivateJwkPath) ? _fileHandler.ReadAllText(existingPrivateJwkPath) : string.Empty;
                    if (!string.IsNullOrWhiteSpace(privateKey))
                    {
                        var result = await _helseIdService.ReadClientSecretExpiration(new ClientConfiguration(clientId, privateKey), authority, baseAddress);

                        return result.HandleResponse(
                            onSuccess: value =>
                            {
                                if (value.SelectedSecret != null && value.SelectedSecret.ExpirationDate.HasValue)
                                {
                                    var epochTime = ((DateTimeOffset)value.SelectedSecret.ExpirationDate.Value).ToUnixTimeSeconds();
                                    _logger.LogDebug("Kid: {Kid}", value.SelectedSecret?.KeyId);
                                    _logger.LogInformation("{EpochTime}", epochTime);
                                    return 0;
                                }

                                _logger.LogError("No secret found with matching Kid.");
                                return 1;
                            },
                            onError: (result) =>
                            {
                                var allMessages = string.Join("; ", result.Errors.Select(e => e.ToString()));
                                _logger.LogDebug("Details: {Details}", allMessages);
                                return 1;
                            });
                    }
                    else
                    {
                        _logger.LogError("No private key provided. Either ExistingPrivateJwk or ExistingPrivateJwkPath must be specified.");
                        return 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReadClientSecretExpiration command. Exception type: {ExceptionType}", ex.GetType().Name);
                return 1;
            }
        }
    }
}