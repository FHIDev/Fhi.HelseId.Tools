using System.Net;
using Fhi.Authentication.Tokens;
using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseIdSelvbetjening.Services
{
    internal class HelseIdSelvbetjeningService : IHelseIdSelvbetjeningService
    {
        private readonly SelvbetjeningConfiguration _selvbetjeningConfig;
        private readonly ITokenService _tokenService;
        private readonly ISelvbetjeningApi _selvbetjeningApi;
        private readonly ILogger<HelseIdSelvbetjeningService> _logger;

        public HelseIdSelvbetjeningService(
            IOptions<SelvbetjeningConfiguration> selvbetjeningConfig,
            ITokenService tokenService,
            ISelvbetjeningApi selvbetjeningApi,
            ILogger<HelseIdSelvbetjeningService> logger)
        {
            _selvbetjeningConfig = selvbetjeningConfig.Value;
            _tokenService = tokenService;
            _selvbetjeningApi = selvbetjeningApi;
            _logger = logger;
        }

        public async Task<ClientSecretUpdateResponse> UpdateClientSecret(ClientConfiguration clientToUpdate, string newPublicJwk)
        {
            _logger.LogInformation("Start updating client {@ClientId} with new key.", clientToUpdate.ClientId);
            var dPoPKey = CreateDPoPKey();
            var response = await _tokenService.CreateDPoPToken(clientToUpdate.ClientId, clientToUpdate.Jwk, "nhn:selvbetjening/client", dPoPKey);
            if (!response.IsError && response.AccessToken != null)
            {
                var (ClientSecretUpdate, ProblemDetail) = await _selvbetjeningApi.UpdateClientSecretsAsync(
                    _selvbetjeningConfig.BaseAddress,
                    dPoPKey,
                    response.AccessToken,
                    newPublicJwk);

                if (ProblemDetail != null)
                {
                    _logger.LogError("Failed to update client {@ClientId}. Error: {@ProblemDetail}", clientToUpdate.ClientId, ProblemDetail.Detail);
                    return new ClientSecretUpdateResponse(HttpStatusCode.BadRequest, ProblemDetail.Detail);
                }
                //TODO: improve response with IResult. Should not serialize output
                return new ClientSecretUpdateResponse(HttpStatusCode.OK, ClientSecretUpdate?.Serialize());
            }

            _logger.LogError("Could not update client {@ClientId}. StatusCode: {@StatusCode}  Error: {@Message}", clientToUpdate.ClientId, response.HttpStatusCode, response.Error);
            return new(response.HttpStatusCode, response.Error);
        }

        public async Task<IResult<ClientSecretExpirationResponse, ErrorResult>> ReadClientSecretExpiration(ClientConfiguration clientConfiguration)
        {
            var errorResult = ValidateClientConfiguration(clientConfiguration);
            if (!errorResult.IsValid)
                return new Error<ClientSecretExpirationResponse, ErrorResult>(errorResult);

            var dPoPKey = CreateDPoPKey();
            var response = await _tokenService.CreateDPoPToken(
                clientConfiguration.ClientId,
                clientConfiguration.Jwk,
                "nhn:selvbetjening/client",
                dPoPKey);

            if (response.IsError || response.AccessToken is null)
            {
                errorResult.AddError($"Token request failed {response.Error}");
                return new Error<ClientSecretExpirationResponse, ErrorResult>(errorResult);
            }

            var (ClientSecrets, ProblemDetail) = await _selvbetjeningApi.GetClientSecretsAsync(_selvbetjeningConfig.BaseAddress, dPoPKey, response.AccessToken);
            if (ProblemDetail != null)
            {
                errorResult.AddError($"Failed to read client secret expiration: {ProblemDetail.Detail}");
                return new Error<ClientSecretExpirationResponse, ErrorResult>(errorResult);
            }

            var securityKey = new JsonWebKey(clientConfiguration.Jwk);
            var selected = ClientSecrets?.FirstOrDefault(s => s.Kid == securityKey.Kid);
            return new Success<ClientSecretExpirationResponse, ErrorResult>(
                new ClientSecretExpirationResponse()
                {
                    SelectedSecret = selected != null ? new ClientSecret(selected.Expiration, selected.Kid, selected.Origin) : null,
                    AllSecrets = ClientSecrets?.Select(s => new ClientSecret(s.Expiration, s.Kid, s.Origin)).ToList() ?? []
                });
        }
        /// <summary>
        /// Validates client configuration and collects all validation errors
        /// </summary>
        /// <param name="clientConfiguration">The client configuration to validate</param>
        /// <returns>A validation result containing any errors found</returns>
        private static ErrorResult ValidateClientConfiguration(ClientConfiguration? clientConfiguration)
        {
            var validationResult = new ErrorResult();

            if (clientConfiguration == null)
            {
                validationResult.AddError("Client configuration cannot be null");
                return validationResult;
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

        private static string CreateDPoPKey()
        {
            var key = JwkGenerator.GenerateRsaJwk();
            return key.PrivateKey;
        }
    }
}
