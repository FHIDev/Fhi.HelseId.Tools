using System.Net;
using Fhi.Authentication.Tokens;
using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.Extensions;
using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseIdSelvbetjening.Business
{
    internal class HelseIdSelvbetjeningService(
        ITokenService tokenService,
        ISelvbetjeningApi selvbetjeningApi,
        ILogger<HelseIdSelvbetjeningService> logger) : IHelseIdSelvbetjeningService
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly ISelvbetjeningApi _selvbetjeningApi = selvbetjeningApi;
        private readonly ILogger<HelseIdSelvbetjeningService> _logger = logger;

        public async Task<IResult<ClientSecretUpdateResponse, ErrorResult>> UpdateClientSecret(ClientConfiguration clientConfiguration, string authority, string baseAddress, string newPublicJwk)
        {
            var errorResult = ValidateClientConfiguration(clientConfiguration);
            if (!errorResult.IsValid)
                return new Error<ClientSecretUpdateResponse, ErrorResult>(errorResult);

            var dPoPKey = CreateDPoPKey();
            var response = await _tokenService.RequestDPoPToken(
                authority,
                clientConfiguration.ClientId,
                clientConfiguration.Jwk,
                "nhn:selvbetjening/client",
                dPoPKey);

            if (response.IsError || response.AccessToken is null)
            {
                errorResult.AddError($"Token request failed {response.ErrorDescription}");
                return new Error<ClientSecretUpdateResponse, ErrorResult>(errorResult);
            }

            var (ClientSecretUpdate, ProblemDetail) = await _selvbetjeningApi.UpdateClientSecretsAsync(
                    baseAddress,
                    dPoPKey,
                    response.AccessToken,
                    newPublicJwk);

            if (ProblemDetail != null)
            {
                errorResult.AddError($"Failed to update client {@clientConfiguration.ClientId}. Error: {@ProblemDetail.Detail}");
                return new Error<ClientSecretUpdateResponse, ErrorResult>(errorResult);
            }

            return new Success<ClientSecretUpdateResponse, ErrorResult>(
                new ClientSecretUpdateResponse()
                {
                    HttpStatus = HttpStatusCode.OK,
                    Message = ClientSecretUpdate?.Serialize()
                });
        }

        public async Task<IResult<ClientSecretExpirationResponse, ErrorResult>> ReadClientSecretExpiration(ClientConfiguration clientConfiguration, string authority, string baseAddress)
        {
            var errorResult = ValidateClientConfiguration(clientConfiguration);
            if (!errorResult.IsValid)
                return new Error<ClientSecretExpirationResponse, ErrorResult>(errorResult);

            var dPoPKey = CreateDPoPKey();
            var response = await _tokenService.RequestDPoPToken(
                authority,
                clientConfiguration.ClientId,
                clientConfiguration.Jwk,
                "nhn:selvbetjening/client",
                dPoPKey);

            if (response.IsError || response.AccessToken is null)
            {
                errorResult.AddError($"Token request failed {response.ErrorDescription}");
                return new Error<ClientSecretExpirationResponse, ErrorResult>(errorResult);
            }

            var (ClientSecrets, ProblemDetail) = await _selvbetjeningApi.GetClientSecretsAsync(baseAddress, dPoPKey, response.AccessToken);
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
