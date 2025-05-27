using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration
{
    internal class ClientSecretExpirationReaderService
    {
        private readonly ReadClientSecretExpirationParameters _parameters;
        private readonly IHelseIdSelvbetjeningService _helseIdSelvbetjeningService;
        private readonly IFileHandler _fileHandler;
        private readonly ILogger<ClientSecretExpirationReaderService> _logger;

        public ClientSecretExpirationReaderService(ReadClientSecretExpirationParameters parameters,
            IHelseIdSelvbetjeningService helseIdSelvbetjeningService,
            IFileHandler fileHandler,
            ILogger<ClientSecretExpirationReaderService> logger)
        {
            _parameters = parameters;
            _helseIdSelvbetjeningService = helseIdSelvbetjeningService;
            _fileHandler = fileHandler;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Reading client secret expiration for client {@ClientId}", _parameters.ClientId);

                var privateKey = !string.IsNullOrEmpty(_parameters.ExistingPrivateJwk) ? _parameters.ExistingPrivateJwk :
                    !string.IsNullOrEmpty(_parameters.ExistingPrivateJwkPath) ? _fileHandler.ReadAllText(_parameters.ExistingPrivateJwkPath) : string.Empty;

                if (!string.IsNullOrEmpty(privateKey))
                {
                    _logger.LogInformation("Using private key for authentication");
                    var response = await _helseIdSelvbetjeningService.ReadClientSecretExpiration(new ClientConfiguration(_parameters.ClientId, privateKey));
                    
                    if (response.HttpStatus == System.Net.HttpStatusCode.OK)
                    {
                        if (response.ExpirationDate.HasValue)
                        {
                            _logger.LogInformation("Successfully retrieved expiration date: {ExpirationDate}", response.ExpirationDate.Value);
                            Console.WriteLine($"Client secret expiration date: {response.ExpirationDate.Value:yyyy-MM-dd HH:mm:ss}");
                        }
                        else
                        {
                            _logger.LogWarning("Expiration date not found in successful response");
                            Console.WriteLine("Client secret expiration date not available in response");
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to read expiration. Status: {Status}, Message: {Message}", response.HttpStatus, response.Message);
                        Console.WriteLine($"Failed to read client secret expiration: {response.Message}");
                    }
                }
                else
                {
                    _logger.LogError("No private key provided. Either ExistingPrivateJwk or ExistingPrivateJwkPath must be specified.");
                    Console.WriteLine("Error: No private key provided. Either --ExistingPrivateJwk or --ExistingPrivateJwkPath must be specified.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading client secret expiration");
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
