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
