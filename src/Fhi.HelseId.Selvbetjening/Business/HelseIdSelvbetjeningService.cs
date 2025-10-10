using System.Net;
using System.Text.Json;
using Fhi.Authentication.Tokens;
using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.Extensions;
using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using static Fhi.HelseIdSelvbetjening.Business.Models.ErrorResult;

namespace Fhi.HelseIdSelvbetjening.Business
{
    internal class HelseIdSelvbetjeningService(
        ITokenService tokenService,
        ISelvbetjeningApi selvbetjeningApi) : IHelseIdSelvbetjeningService
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly ISelvbetjeningApi _selvbetjeningApi = selvbetjeningApi;

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
                var errorMessageText = $"Token request failed: {(string.IsNullOrEmpty(response.ErrorDescription) ? "No Error message provided by API" : response.ErrorDescription)}";
                var error = new ErrorMessage(errorMessageText, HttpStatusCode.BadRequest, "error");
                errorResult.AddError(error);
                return new Error<ClientSecretUpdateResponse, ErrorResult>(errorResult);
            }

            var (ClientSecretUpdate, ProblemDetail) = await _selvbetjeningApi.UpdateClientSecretsAsync(
                    baseAddress,
                    dPoPKey,
                    response.AccessToken,
                    newPublicJwk);

            if (ProblemDetail != null)
            {
                var error = new ErrorMessage($"Failed to update client {@clientConfiguration.ClientId}. Error: {@ProblemDetail.Detail}", HttpStatusCode.BadRequest, "error");
                errorResult.AddError(error);
                return new Error<ClientSecretUpdateResponse, ErrorResult>(errorResult);
            }

            if (ClientSecretUpdate is null)
            {
                var error = new ErrorMessage($"Error occured while updating client {@clientConfiguration.ClientId}. Error: HelseID did not return with the expected content. Check if client was updated before retrying.", HttpStatusCode.BadGateway, "error");
                errorResult.AddError(error);
                return new Error<ClientSecretUpdateResponse, ErrorResult>(errorResult);
            }

            return new Success<ClientSecretUpdateResponse, ErrorResult>(
                new ClientSecretUpdateResponse()
                {
                    ExpirationDate = ClientSecretUpdate.ToString(),
                    ClientId = clientConfiguration.ClientId,
                    NewKeyId = ExtractKid(newPublicJwk)
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
                var error = new ErrorMessage($"Token request failed {response.ErrorDescription}", HttpStatusCode.BadRequest, "error");
                errorResult.AddError(error);
                return new Error<ClientSecretExpirationResponse, ErrorResult>(errorResult);
            }

            var (ClientSecrets, ProblemDetail) = await _selvbetjeningApi.GetClientSecretsAsync(baseAddress, dPoPKey, response.AccessToken);
            if (ProblemDetail != null)
            {
                var error = new ErrorMessage($"Failed to read client secret expiration: {ProblemDetail.Detail}", HttpStatusCode.BadRequest, "error");
                errorResult.AddError(error);
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
                var error = new ErrorMessage("Client configuration cannot be null", HttpStatusCode.BadRequest, "error");
                validationResult.AddError(error);
                return validationResult;
            }

            if (string.IsNullOrWhiteSpace(clientConfiguration.ClientId))
            {
                var error = new ErrorMessage("ClientId cannot be null or empty", HttpStatusCode.BadRequest, "error");
                validationResult.AddError(error);
            }

            if (string.IsNullOrWhiteSpace(clientConfiguration.Jwk))
            {
                var error = new ErrorMessage("Jwk cannot be null or empty", HttpStatusCode.BadRequest, "error");
                validationResult.AddError(error);
            }

            return validationResult;
        }

        private static string CreateDPoPKey()
        {
            var key = JwkGenerator.GenerateRsaJwk();
            return key.PrivateKey;
        }

        /// <summary>
        /// Takes a jwk key and returns just the kid
        /// </summary>
        /// <param name="jwkJson"></param>
        /// <returns></returns>
        private static string ExtractKid(string jwkJson)
        {
            using var doc = JsonDocument.Parse(jwkJson);
            if (doc.RootElement.TryGetProperty("kid", out var kidElement))
            {
                return kidElement.GetString()!;
            }
            return string.Empty;
        }
    }
}
