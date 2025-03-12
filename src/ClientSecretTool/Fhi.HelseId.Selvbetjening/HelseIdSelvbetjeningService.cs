using Duende.IdentityModel.Client;
using Fhi.HelseId.Selvbetjening.Http;
using Fhi.HelseId.Selvbetjening.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fhi.HelseId.Selvbetjening
{
    public class HelseIdSelvbetjeningService : IHelseIdSelvbetjeningService
    {
        private readonly SelvbetjeningConfiguration _selvbetjeningConfig;

        public HelseIdSelvbetjeningService(IOptions<SelvbetjeningConfiguration> selvbetjeningConfig)
        {
            _selvbetjeningConfig = selvbetjeningConfig.Value;
        }

        public async Task<ClientSecretUpdateResponse?> UpdateClientSecret(ClientConfiguration clientToUpdate, string newPublicJwk)
        {
            var client = new HttpClient();
            /*** Get HelseID metadata from discovery endpoint ***/
            var discovery = await client.GetDiscoveryDocumentAsync(_selvbetjeningConfig.Authority);
            if (discovery.IsError || discovery.Issuer is null || discovery.TokenEndpoint is null) throw new Exception(discovery.Error);

            /*** Create token (access and dpop) from client to update***/
            var jwkKey = new JsonWebKey(clientToUpdate.Jwk);
            TokenResponse response = await CreateToken(clientToUpdate, client, discovery, jwkKey);

            /*** Get client secret info (public jwk) in NHN selvbetjening ***/
            var uri = new Uri(new Uri(_selvbetjeningConfig.BaseAddress), _selvbetjeningConfig.ClientSecretEndpoint);
            var requestMessage = new HttpRequestMessageBuilder()
                .Create(HttpMethod.Get, uri)
                .WithDpop(uri.ToString(), HttpMethod.Get.ToString(), clientToUpdate.Jwk, jwkKey.Alg, response.AccessToken)
                .Build();

            var clientSecretUpdateResponse = await client.SendAsync(requestMessage);
            return new ClientSecretUpdateResponse(clientSecretUpdateResponse.StatusCode, await clientSecretUpdateResponse.Content.ReadAsStringAsync());
        }

        public Task<ClientSecretStatusResponse?>? GetClientSecretStatus(ClientConfiguration clientToUpdate)
        {
            return null;
        }

        private static async Task<TokenResponse> CreateToken(ClientConfiguration clientToUpdate, HttpClient client, DiscoveryDocumentResponse discovery, JsonWebKey jwkKey)
        {
            var nonceRequest = new ClientCredentialRequestBuilder()
                .Create(discovery.TokenEndpoint, clientToUpdate.ClientId)
                .WithDPoP(discovery.TokenEndpoint, HttpMethod.Post.ToString(), clientToUpdate.Jwk, jwkKey.Alg)
                .WithClientAssertion(discovery.Issuer, clientToUpdate.Jwk)
                .WithScope("nhn:selvbetjening/client")
                .Build();
            var response = await client.RequestClientCredentialsTokenAsync(nonceRequest);

            if (response.Error == "use_dpop_nonce" && response.DPoPNonce is not null)
            {
                var tokenRequest = new ClientCredentialRequestBuilder()
                    .Create(discovery.TokenEndpoint, clientToUpdate.ClientId)
                    .WithDPoPNonce(discovery.TokenEndpoint, HttpMethod.Post.ToString(), clientToUpdate.Jwk, jwkKey.Alg, response.DPoPNonce)
                    .WithClientAssertion(discovery.Issuer, clientToUpdate.Jwk)
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

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                IgnoreReadOnlyProperties = true,
            };
        }
    }
}
