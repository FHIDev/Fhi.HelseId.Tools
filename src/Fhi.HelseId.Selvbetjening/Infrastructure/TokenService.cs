using System.Net;
using Duende.IdentityModel.Client;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fhi.HelseIdSelvbetjening.Infrastructure
{
    internal record TokenResponse(string? AccessToken, bool IsError, string? Error, HttpStatusCode HttpStatusCode);
    internal interface ITokenService
    {
        /// <summary>
        /// Create DPoP token
        /// </summary>
        /// <param name="clientId">Client Identifier</param>
        /// <param name="jwk">The private json web key for client assertion</param>
        /// <param name="scopes">Separated list of scopes</param>
        /// <param name="dPopJwk">The private json web key for DPoP</param>
        /// <returns></returns>
        public Task<TokenResponse> CreateDPoPToken(string clientId, string jwk, string scopes, string dPopJwk);
    }

    internal class TokenService(
        ILogger<TokenService> Logger,
        IOptions<SelvbetjeningConfiguration> Options,
        IHttpClientFactory HttpClientFactory) : ITokenService
    {
        public async Task<TokenResponse> CreateDPoPToken(
           string clientId,
           string jwk,
           string scopes,
           string dPopJwk)
        {
            var client = HttpClientFactory.CreateClient();
            Logger.LogInformation("Get metadata from discovery endpoint from Authority {@Authority}", Options.Value.Authority);
            var discovery = await client.GetDiscoveryDocumentAsync(Options.Value.Authority);
            if (discovery.IsError || discovery.Issuer is null || discovery.TokenEndpoint is null) throw new Exception(discovery.Error);

            Logger.LogDebug("DPoP token nonce request");
            var nonceRequest = new ClientCredentialRequestBuilder()
                .Create(discovery.TokenEndpoint, clientId)
                .WithDPoP(discovery.TokenEndpoint, HttpMethod.Post.ToString(), dPopJwk, "PS256")
                .WithClientAssertion(discovery.Issuer, jwk)
                .WithScope(scopes)
                .Build();
            var response = await client.RequestClientCredentialsTokenAsync(nonceRequest);

            if (response.Error == "use_dpop_nonce" && response.DPoPNonce is not null)
            {
                Logger.LogDebug("DPoP token request with nonce");
                var tokenRequest = new ClientCredentialRequestBuilder()
                    .Create(discovery.TokenEndpoint, clientId)
                    .WithDPoPNonce(discovery.TokenEndpoint, HttpMethod.Post.ToString(), dPopJwk, "PS256", response.DPoPNonce)
                    .WithClientAssertion(discovery.Issuer, jwk)
                    .WithScope(scopes)
                    .Build();
                response = await client.RequestClientCredentialsTokenAsync(tokenRequest);
            }
            else if (response.IsError)
            {
                Logger.LogError(response.Error);
                return new TokenResponse(response.AccessToken, true, response.Error, response.HttpStatusCode);
            }

            Logger.LogInformation("Successfully get DPoP token for client {clientId}", clientId);

            return new TokenResponse(response.AccessToken, false, string.Empty, response.HttpStatusCode);
        }
    }
}
