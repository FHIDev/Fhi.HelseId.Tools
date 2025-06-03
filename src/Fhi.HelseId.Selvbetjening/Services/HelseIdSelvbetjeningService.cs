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
            try
            {
                if (clientConfiguration == null)
                {
                    throw new ArgumentNullException(nameof(clientConfiguration));
                }
                if (string.IsNullOrWhiteSpace(clientConfiguration.ClientId))
                {
                    throw new ArgumentException("ClientId cannot be null or empty", nameof(clientConfiguration));
                }
                if (string.IsNullOrWhiteSpace(clientConfiguration.Jwk))
                {
                    throw new ArgumentException("Jwk cannot be null or empty", nameof(clientConfiguration));
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
                        DateTime? expirationDate = null;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                var jsonDoc = JsonDocument.Parse(content);
                                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                                {
                                    string? clientKid = null;
                                    try
                                    {
                                        var clientJwkDoc = JsonDocument.Parse(clientConfiguration.Jwk);
                                        if (clientJwkDoc.RootElement.TryGetProperty("kid", out var kidProperty))
                                        {
                                            clientKid = kidProperty.GetString();
                                        }
                                    }
                                    catch (JsonException)
                                    {
                                        _logger.LogWarning("Could not parse client JWK to extract kid for matching");
                                    }

                                    DateTime? latestExpiration = null;
                                    DateTime? matchingKidExpiration = null;

                                    foreach (var secretElement in jsonDoc.RootElement.EnumerateArray())
                                    {
                                        if (secretElement.TryGetProperty("expiration", out var expProperty) &&
                                            expProperty.ValueKind != JsonValueKind.Null)
                                        {
                                            if (DateTimeOffset.TryParse(expProperty.GetString(), out var parsedDateOffset))
                                            {
                                                var currentExpiration = parsedDateOffset.UtcDateTime;

                                                if (!string.IsNullOrEmpty(clientKid) &&
                                                    secretElement.TryGetProperty("kid", out var responseKidProperty))
                                                {
                                                    var responseKid = responseKidProperty.GetString();
                                                    if (string.Equals(clientKid, responseKid, StringComparison.Ordinal))
                                                    {
                                                        matchingKidExpiration = currentExpiration;
                                                        _logger.LogInformation("Found matching kid '{Kid}' with expiration {Expiration}",
                                                            clientKid, currentExpiration);
                                                    }
                                                }

                                                if (latestExpiration == null || currentExpiration > latestExpiration)
                                                {
                                                    latestExpiration = currentExpiration;
                                                }
                                            }
                                        }
                                    }

                                    expirationDate = matchingKidExpiration ?? latestExpiration;

                                    if (matchingKidExpiration.HasValue)
                                    {
                                        _logger.LogInformation("Using expiration from matching kid: {Expiration}", matchingKidExpiration);
                                    }
                                    else if (latestExpiration.HasValue)
                                    {
                                        _logger.LogInformation("No matching kid found, using latest expiration: {Expiration}", latestExpiration);
                                    }
                                }
                                else
                                {
                                    if (jsonDoc.RootElement.TryGetProperty("expirationDate", out var expDateElement))
                                    {
                                        if (DateTimeOffset.TryParse(expDateElement.GetString(), out var parsedDateOffset))
                                        {
                                            expirationDate = parsedDateOffset.UtcDateTime;
                                        }
                                    }
                                    else if (jsonDoc.RootElement.TryGetProperty("expiration", out var expElement))
                                    {
                                        if (expElement.ValueKind != JsonValueKind.Null && DateTimeOffset.TryParse(expElement.GetString(), out var parsedDateOffset))
                                        {
                                            expirationDate = parsedDateOffset.UtcDateTime;
                                        }
                                    }
                                    else if (jsonDoc.RootElement.TryGetProperty("exp", out var expTimestampElement))
                                    {
                                        if (expTimestampElement.TryGetInt64(out var unixTimestamp))
                                        {
                                            expirationDate = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
                                        }
                                    }
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning("Could not parse expiration date from response: {Error}", ex.Message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Unexpected error parsing expiration date from response: {Error}", ex.Message);
                        }

                        return new ClientSecretExpirationResponse(clientSecretExpirationResponse.StatusCode, "Successfully retrieved client secret expiration", expirationDate);
                    }

                    return new ClientSecretExpirationResponse(clientSecretExpirationResponse.StatusCode, content);
                }

                _logger.LogError("Could not read client secret expiration for {@ClientId}. StatusCode: {@StatusCode}  Error: {@Message}", clientConfiguration.ClientId, response.HttpStatusCode, response.Error);
                return new(response.HttpStatusCode, response.Error);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when reading client secret expiration for {@ClientId}", clientConfiguration.ClientId);
                return new(System.Net.HttpStatusCode.ServiceUnavailable, $"HTTP request failed: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout when reading client secret expiration for {@ClientId}", clientConfiguration.ClientId);
                return new(System.Net.HttpStatusCode.RequestTimeout, $"Request timeout: {ex.Message}");
            }
            catch (UriFormatException ex)
            {
                _logger.LogError(ex, "Invalid URI configuration when reading client secret expiration for {@ClientId}", clientConfiguration.ClientId);
                return new(System.Net.HttpStatusCode.InternalServerError, $"Invalid URI configuration: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument when reading client secret expiration for {@ClientId}", clientConfiguration!.ClientId);
                return new(System.Net.HttpStatusCode.BadRequest, $"Invalid argument: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when reading client secret expiration for {@ClientId}", clientConfiguration.ClientId);
                return new(System.Net.HttpStatusCode.InternalServerError, $"Unexpected error: {ex.Message}");
            }
        }

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                IgnoreReadOnlyProperties = true,
            };
        }

        private static string CreateDPoPKey()
        {
            var key = JwkGenerator.GenerateRsaJwk();
            return key.PrivateKey;
        }
    }
}
