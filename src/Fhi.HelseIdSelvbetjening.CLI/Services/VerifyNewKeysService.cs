

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Fhi.HelseIdSelvbetjening.Http;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Fhi.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseIdSelvbetjening.CLI.Services
{
    public interface IVerifyNewKeysService
    {
        Task<bool> ExecuteAsync(string clientId, string privateJwk);
    }

    public class VerifyNewKeysService : IVerifyNewKeysService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SelvbetjeningConfiguration _selvbetjeningConfiguration;
        private readonly ILogger<VerifyNewKeysService> _logger;

        public VerifyNewKeysService(
            IHttpClientFactory httpClientFactory,
            IOptions<SelvbetjeningConfiguration> selvbetjeningConfigurationOptions,
            ILogger<VerifyNewKeysService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _selvbetjeningConfiguration = selvbetjeningConfigurationOptions.Value;
            _logger = logger;
        }

        public async Task<bool> ExecuteAsync(string clientId, string privateJwk)
        {
            _logger.LogInformation("Attempting to verify new key for client ID: {ClientId}", clientId);

            try
            {
                var client = _httpClientFactory.CreateClient();
                var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                {
                    Address = _selvbetjeningConfiguration.Authority,
                    Policy = { RequireHttps = true }
                });

                if (disco.IsError || disco.Issuer is null || disco.TokenEndpoint is null)
                {
                    _logger.LogError("Discovery document error: {Error}", disco.Error);
                    return false;
                }

                _logger.LogInformation("Successfully retrieved discovery document from: {TokenEndpoint}", disco.TokenEndpoint);

                // Request token using client assertion
                var clientConfiguration = new ClientConfiguration(clientId, privateJwk);
                var tokenResponse = await CreateToken(
                    clientConfiguration,
                    client,
                    disco.TokenEndpoint,
                    disco.Issuer,
                    CreateDPoPKey());

                if (tokenResponse.IsError)
                {
                    _logger.LogError("Token request error: {Error}. Description: {ErrorDescription}", tokenResponse.Error, tokenResponse.ErrorDescription);
                    _logger.LogError("Raw response: {Raw}", tokenResponse.Raw);
                    return false;
                }

                _logger.LogInformation("Successfully obtained token using the new key. Access Token: {AccessToken}", tokenResponse.AccessToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying the new key.");
                return false;
            }
        }
        
        private static string CreateDPoPKey()
        {
            var key = JwkGenerator.GenerateRsaJwk();

            return key.PrivateKey;
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
    }
}
