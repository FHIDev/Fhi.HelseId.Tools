using Fhi.HelseIdSelvbetjening.CLI.Services;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

        public async Task<int> ExecuteAsync()
        {
            _logger.LogInformation("Reading client secret expiration for client {@ClientId}", _parameters.ClientId);
            var privateKey = !string.IsNullOrWhiteSpace(_parameters.ExistingPrivateJwk) ? _parameters.ExistingPrivateJwk :
                !string.IsNullOrWhiteSpace(_parameters.ExistingPrivateJwkPath) ? _fileHandler.ReadAllText(_parameters.ExistingPrivateJwkPath) : string.Empty;
            if (!string.IsNullOrWhiteSpace(privateKey))
            {
                // Extract kid from JWK for logging
                var kid = ExtractKidFromJwk(privateKey);
                _logger.LogInformation("Using private key for authentication with Client ID: {ClientId}, Key ID (kid): {Kid}", _parameters.ClientId, kid ?? "not specified");

                var response = await _helseIdSelvbetjeningService.ReadClientSecretExpiration(new ClientConfiguration(_parameters.ClientId, privateKey));
                if (response.HttpStatus == System.Net.HttpStatusCode.OK)
                {
                    if (response.ExpirationDate != DateTime.MinValue)
                    {
                        var epochTime = ((DateTimeOffset)response.ExpirationDate).ToUnixTimeSeconds();
                        _logger.LogInformation("{EpochTime}", epochTime);
                        return 0;
                    }
                    else
                    {
                        _logger.LogError("Expiration date not found in successful response");
                        return 1;
                    }
                }
                else
                {
                    if (response.HasValidationErrors)
                    {
                        _logger.LogError("Validation failed: {ValidationErrors}", string.Join(", ", response.ValidationErrors!));
                    }
                    else
                    {
                        _logger.LogError("Failed to read expiration. Status: {Status}, Message: {Message}", response.HttpStatus, response.Message);
                    }
                    return 1;
                }
            }
            else
            {
                _logger.LogError("No private key provided. Either ExistingPrivateJwk or ExistingPrivateJwkPath must be specified.");
                return 1;
            }
        }

        /// <summary>
        /// Extracts the 'kid' (key identifier) from a JWK JSON string.
        /// </summary>
        /// <param name="jwkJson">The JWK as a JSON string</param>
        /// <returns>The kid value if found, otherwise null</returns>
        private static string? ExtractKidFromJwk(string jwkJson)
        {
            try
            {
                var jwk = JsonSerializer.Deserialize<JsonWebKey>(jwkJson, JsonHelper.CreateJsonSerializerOptions());
                return jwk?.Kid;
            }
            catch
            {
                return null;
            }
        }
    }
}
