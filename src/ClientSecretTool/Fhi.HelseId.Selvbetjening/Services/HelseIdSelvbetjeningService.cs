using Duende.IdentityModel.Client;
using Fhi.HelseId.Selvbetjening.Http;
using Fhi.HelseId.Selvbetjening.Services.Models;
using Fhi.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fhi.HelseId.Selvbetjening.Services
{
    public class HelseIdSelvbetjeningService : IHelseIdSelvbetjeningService
    {
        private readonly SelvbetjeningConfiguration _selvbetjeningConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HelseIdSelvbetjeningService> _logger;

        public HelseIdSelvbetjeningService(
            IOptions<SelvbetjeningConfiguration> selvbetjeningConfig,
            IHttpClientFactory httpClientFactory,
            ILogger<HelseIdSelvbetjeningService> logger)
        {
            _selvbetjeningConfig = selvbetjeningConfig.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ClientSecretUpdateResponse> UpdateClientSecret(ClientConfiguration clientToUpdate, string newPublicJwk)
        {
            try
            {
                _logger.LogInformation("Start updating client {@ClientId} with new key.", clientToUpdate.ClientId);
                _logger.LogInformation("Selvbetjening parameters: {@Paramters}", _selvbetjeningConfig);

                var client = _httpClientFactory.CreateClient();
                _logger.LogInformation("/*** Get HelseID metadata from discovery endpoint from {@Authority} ***/", _selvbetjeningConfig.Authority);
                var discovery = await client.GetDiscoveryDocumentAsync(_selvbetjeningConfig.Authority);
                if (discovery.IsError || discovery.Issuer is null || discovery.TokenEndpoint is null) throw new Exception(discovery.Error);

                _logger.LogInformation("/*** Create token (access and dpop) from client {@ClientId} to update***/", clientToUpdate.ClientId);
                var dPoPKey = CreateDPoPKey();
                TokenResponse response = await CreateToken(
                    clientToUpdate,
                    client,
                    discovery.TokenEndpoint,
                    discovery.Issuer,
                    dPoPKey);

                if (!response.IsError && response.AccessToken is not null)
                {
                    _logger.LogInformation($"/*** Get client secret info (public jwk) in NHN selvbetjening ***/");
                    var uri = new Uri(new Uri(_selvbetjeningConfig.BaseAddress), _selvbetjeningConfig.ClientSecretEndpoint);

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
                       .WithDpop(uri.ToString(), HttpMethod.Post.ToString(), dPoPKey, "PS256", response.AccessToken)
                       .WithContent(JsonContent.Create(newPublicJwk, options: CreateJsonSerializerOptions()))
                       .WithHeader("Accept", "application/json");

                    var clientSecretUpdateResponse = await client.SendAsync(requestMessage);

                    _logger.LogInformation("Client update response: {@StatusCode}", clientSecretUpdateResponse.StatusCode);
                    _logger.LogInformation("Response: {@Response}", await clientSecretUpdateResponse.Content.ReadAsStringAsync());

                    return new ClientSecretUpdateResponse(clientSecretUpdateResponse.StatusCode, "successfully updated client secret");
                }
                _logger.LogInformation("Token response: {@StatusCode}", response.HttpStatusCode);
                _logger.LogInformation("{@ErrorDescription}", response.Error);

                return new(response.HttpStatusCode, response.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
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

        private async Task<TokenResponse> CreateToken(
            ClientConfiguration clientToUpdate,
            HttpClient client,
            string tokenEndpoint,
            string issuer,
            string dPopJwk)
        {
            _logger.LogDebug("Dpop token nonce request");
            var nonceRequest = new ClientCredentialRequestBuilder()
                .Create(tokenEndpoint, clientToUpdate.ClientId)
                .WithDPoP(tokenEndpoint, HttpMethod.Post.ToString(), dPopJwk, "PS256")
                .WithClientAssertion(issuer, clientToUpdate.Jwk)
                .WithScope("nhn:selvbetjening/client")
                .Build();
            var response = await client.RequestClientCredentialsTokenAsync(nonceRequest);

            if (response.Error == "use_dpop_nonce" && response.DPoPNonce is not null)
            {
                _logger.LogDebug("Dpop token request with nonce");
                var tokenRequest = new ClientCredentialRequestBuilder()
                    .Create(tokenEndpoint, clientToUpdate.ClientId)
                    .WithDPoPNonce(tokenEndpoint, HttpMethod.Post.ToString(), dPopJwk, "PS256", response.DPoPNonce)
                    .WithClientAssertion(issuer, clientToUpdate.Jwk)
                    .WithScope("nhn:selvbetjening/client")
                    .Build();
                response = await client.RequestClientCredentialsTokenAsync(tokenRequest);
            }
            else if (response.IsError)
            {
                throw new Exception(response.Error);
            }

            return response;
        }

        private static string CreateDPoPKey()
        {
            var key = JwkGenerator.GenerateRsaJwk();

            return key.PrivateKey;
        }
    }
}
