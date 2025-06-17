using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fhi.Authentication.Tokens;
using Fhi.HelseIdSelvbetjening.Http;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fhi.HelseIdSelvbetjening.Services
{
    internal class HelseIdSelvbetjeningService : IHelseIdSelvbetjeningService
    {
        private readonly SelvbetjeningConfiguration _selvbetjeningConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<HelseIdSelvbetjeningService> _logger;

        public HelseIdSelvbetjeningService(
            IOptions<SelvbetjeningConfiguration> selvbetjeningConfig,
            IHttpClientFactory httpClientFactory,
            ITokenService tokenService,
            ILogger<HelseIdSelvbetjeningService> logger)
        {
            _selvbetjeningConfig = selvbetjeningConfig.Value;
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<ClientSecretUpdateResponse> UpdateClientSecret(ClientConfiguration clientToUpdate, string newPublicJwk)
        {
            _logger.LogInformation("Start updating client {@ClientId} with new key.", clientToUpdate.ClientId);
            var dPoPKey = CreateDPoPKey();
            var response = await _tokenService.CreateDPoPToken(clientToUpdate.ClientId, clientToUpdate.Jwk, "nhn:selvbetjening/client", dPoPKey);
            if (!response.IsError && response.AccessToken is not null)
            {
                var uri = new Uri(new Uri(_selvbetjeningConfig.BaseAddress), _selvbetjeningConfig.ClientSecretEndpoint);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
                    .WithDpop(uri.ToString(), HttpMethod.Post.ToString(), dPoPKey, "PS256", response.AccessToken)
                    .WithContent(JsonContent.Create(newPublicJwk, options: CreateJsonSerializerOptions()))
                    .WithHeader("Accept", "application/json");
                var client = _httpClientFactory.CreateClient();
                var clientSecretUpdateResponse = await client.SendAsync(requestMessage);

                _logger.LogInformation(
                    "Client update response: {@StatusCode}  Response: {@Response}",
                    clientSecretUpdateResponse.StatusCode,
                        await clientSecretUpdateResponse.Content.ReadAsStringAsync());

                return new ClientSecretUpdateResponse(clientSecretUpdateResponse.StatusCode, "successfully updated client secret");
            }
            _logger.LogError("Could not update client {@ClientId}. StatusCode: {@StatusCode}  Error: {@Message}", clientToUpdate.ClientId, response.HttpStatusCode, response.Error);
            return new(response.HttpStatusCode, response.Error);
        }
        
        public async Task<ClientSecretExpirationResponse> ReadClientSecretExpiration(ClientConfiguration clientConfiguration)
        {
            // Validate input and collect all errors
            var validationResult = ValidateClientConfiguration(clientConfiguration);
            if (!validationResult.IsValid)
            {
                return ClientSecretExpirationResponse.FromValidationErrors(validationResult);
            }

            _logger.LogInformation("Reading client secret expiration for client {@ClientId}.", clientConfiguration.ClientId);
            var dPoPKey = CreateDPoPKey();
            var response = await _tokenService.CreateDPoPToken(
                clientConfiguration.ClientId,
                clientConfiguration.Jwk,
                "nhn:selvbetjening/client",
                dPoPKey).ConfigureAwait(false);
            
            if (response is { IsError: false, AccessToken: not null })
            {
                var uri = new Uri(new Uri(_selvbetjeningConfig.BaseAddress), _selvbetjeningConfig.ClientSecretEndpoint);
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri)
                    .WithDpop(uri.ToString(), HttpMethod.Get.ToString(), dPoPKey, "PS256", response.AccessToken)
                    .WithHeader("Accept", "application/json");
                var client = _httpClientFactory.CreateClient();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var clientSecretExpirationResponse = await client.SendAsync(requestMessage, cts.Token).ConfigureAwait(false);

                var content = await clientSecretExpirationResponse.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);

                _logger.LogInformation(
                    "Client secret expiration response: {@StatusCode}  Response: {@Response}",
                    clientSecretExpirationResponse.StatusCode,
                    content);
                if (clientSecretExpirationResponse.IsSuccessStatusCode)
                {
                    DateTime expirationDate = ParseClientSecretExpirationResponse(content, clientConfiguration.Jwk);
                    return new ClientSecretExpirationResponse(clientSecretExpirationResponse.StatusCode, "Successfully retrieved client secret expiration", expirationDate);
                }
                return new ClientSecretExpirationResponse(clientSecretExpirationResponse.StatusCode, content, DateTime.MinValue);
            }
            _logger.LogError("Could not read client secret expiration for {@ClientId}. StatusCode: {@StatusCode}  Error: {@Message}", clientConfiguration.ClientId, response.HttpStatusCode, response.Error);
            return new(response.HttpStatusCode, response.Error, DateTime.MinValue);
        }

        /// <summary>
        /// Validates client configuration and collects all validation errors
        /// </summary>
        /// <param name="clientConfiguration">The client configuration to validate</param>
        /// <returns>A validation result containing any errors found</returns>
        private static ValidationResult ValidateClientConfiguration(ClientConfiguration? clientConfiguration)
        {
            var validationResult = new ValidationResult();

            if (clientConfiguration == null)
            {
                validationResult.AddError("Client configuration cannot be null");
                return validationResult; // Short circuit since we can't validate further
            }

            if (string.IsNullOrWhiteSpace(clientConfiguration.ClientId))
            {
                validationResult.AddError("ClientId cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(clientConfiguration.Jwk))
            {
                validationResult.AddError("Jwk cannot be null or empty");
            }

            return validationResult;
        }

        /// <summary>
        /// Parses the client secret expiration response JSON and extracts the expiration date
        /// </summary>
        /// <param name="jsonContent">The JSON response content</param>
        /// <param name="clientJwk">The client's JWK to match against</param>
        /// <returns>The expiration date if found, DateTime.MinValue otherwise</returns>
        private static DateTime ParseClientSecretExpirationResponse(string jsonContent, string clientJwk)
        {
            // Check if JSON starts with '[' to determine if it's an array
            var trimmedContent = jsonContent.Trim();
            if (trimmedContent.StartsWith('['))
            {
                var arrayResponse = ParseArrayResponse(jsonContent);
                if (arrayResponse?.Any() == true)
                {
                    return FindExpirationDateInSecrets(arrayResponse, clientJwk);
                }
            }
            else
            {
                var singleResponse = ParseSingleResponse(jsonContent);
                return singleResponse?.GetEffectiveExpirationDate() ?? DateTime.MinValue;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Parses JSON content as an array of client secrets
        /// </summary>
        /// <param name="jsonContent">The JSON content to parse</param>
        /// <returns>Array of client secrets or null if parsing fails</returns>
        private static ClientSecret[]? ParseArrayResponse(string jsonContent)
        {
            return JsonSerializer.Deserialize<ClientSecret[]>(jsonContent, CreateJsonSerializerOptions());
        }

        /// <summary>
        /// Parses JSON content as a single expiration response
        /// </summary>
        /// <param name="jsonContent">The JSON content to parse</param>
        /// <returns>Single expiration response or null if parsing fails</returns>
        private static SingleExpirationResponse? ParseSingleResponse(string jsonContent)
        {
            return JsonSerializer.Deserialize<SingleExpirationResponse>(jsonContent, CreateJsonSerializerOptions());
        }

        /// <summary>
        /// Finds the expiration date for a matching client secret in an array
        /// </summary>
        /// <param name="secrets">Array of client secrets</param>
        /// <param name="clientJwk">The client JWK to match against</param>
        /// <returns>The expiration date if a matching secret is found, DateTime.MinValue otherwise</returns>
        private static DateTime FindExpirationDateInSecrets(ClientSecret[] secrets, string clientJwk)
        {
            // Try to extract kid from client JWK - let JSON exceptions bubble up
            string clientKid = ExtractKidFromJwk(clientJwk);

            // If we have a valid kid, try to find matching secret
            if (clientKid != string.Empty)
            {
                foreach (var secret in secrets)
                {
                    if (secret.Kid == clientKid)
                    {
                        return secret.Expiration ?? DateTime.MinValue;
                    }
                }
            }

            // If no matching kid found, return the latest (most recent) expiration date available
            return secrets.Where(s => s.Expiration.HasValue)
                         .OrderByDescending(s => s.Expiration)
                         .FirstOrDefault()?.Expiration ?? DateTime.MinValue;
        }

        /// <summary>
        /// Extracts the 'kid' (Key ID) from a JWK JSON string
        /// </summary>
        /// <param name="jwkJson">The JWK JSON string</param>
        /// <returns>The kid value if found, empty string otherwise</returns>
        private static string ExtractKidFromJwk(string jwkJson)
        {
            // Basic validation - if it doesn't look like JSON, return empty string
            if (string.IsNullOrWhiteSpace(jwkJson) || !jwkJson.Trim().StartsWith('{'))
            {
                return string.Empty;
            }

            var jwk = JsonSerializer.Deserialize<JsonWebKey>(jwkJson, CreateJsonSerializerOptions());
            return jwk?.Kid ?? string.Empty;
        }

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                IgnoreReadOnlyProperties = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private static string CreateDPoPKey()
        {
            var key = JwkGenerator.GenerateRsaJwk();
            return key.PrivateKey;
        }
    }
}
