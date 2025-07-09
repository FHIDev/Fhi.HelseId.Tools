using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Fhi.Authentication.Tokens;
using Fhi.HelseIdSelvbetjening.Infrastructure.Tokens;

namespace Fhi.HelseIdSelvbetjening.Infrastructure
{
    /// <summary>
    ///  Build ClientCredentialsTokenRequest
    /// </summary>
    internal class ClientCredentialRequestBuilder
    {
        private static ClientCredentialsTokenRequest _request = new();
        /// <summary>
        /// Initializer for ClientCredentialsTokenRequest 
        /// </summary>
        /// <param name="tokenEndpoint"></param>
        /// <param name="clientId"></param>
        /// <param name="grantType"></param>
        /// <param name="credentialStyle"></param>
        /// <returns></returns>
        public ClientCredentialRequestBuilder Create(
            string tokenEndpoint,
            string clientId,
            string grantType = OidcConstants.GrantTypes.ClientCredentials,
            ClientCredentialStyle credentialStyle = ClientCredentialStyle.PostBody)
        {
            _request = new()
            {
                ClientId = clientId,
                Address = tokenEndpoint,
                GrantType = grantType,
                ClientCredentialStyle = credentialStyle
            };

            return this;
        }

        /// <summary>
        ///  Add DPoP with nonce to ClientCredentialsTokenRequest
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="httpMethod"></param>
        /// <param name="privateJwk"></param>
        /// <param name="privateJwkAlg"></param>
        /// <param name="nonce"></param>
        /// <returns></returns>
        public ClientCredentialRequestBuilder WithDPoPNonce(string uri, string httpMethod, string privateJwk, string privateJwkAlg, string nonce)
        {
            var dpopProofAccessToken = DPoPProofGenerator.CreateDPoPProof(
                uri,
                httpMethod,
                privateJwk,
                privateJwkAlg,
                nonce);

            _request.DPoPProofToken = dpopProofAccessToken;

            return this;
        }

        /// <summary>
        /// Add DPoP to ClientCredentialsTokenRequest
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="httpMethod"></param>
        /// <param name="privateJwk"></param>
        /// <param name="privateJwkAlg"></param>
        /// <returns></returns>
        public ClientCredentialRequestBuilder WithDPoP(string uri, string httpMethod, string privateJwk, string privateJwkAlg)
        {
            var dpopProofAccessToken = DPoPProofGenerator.CreateDPoPProof(
                uri,
                httpMethod,
                privateJwk,
                privateJwkAlg);

            _request.DPoPProofToken = dpopProofAccessToken;

            return this;
        }

        /// <summary>
        /// Create Client assertion token from private jwk and add ClientAssertion to ClientCredentialsTokenRequest
        /// </summary>
        /// <param name="issuer"></param>
        /// <param name="privateJwk"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ClientCredentialRequestBuilder WithClientAssertion(string issuer, string privateJwk, string type = OidcConstants.ClientAssertionTypes.JwtBearer)
        {
            var clientAssertion = ClientAssertionTokenHandler.CreateJwtToken(issuer, _request.ClientId, privateJwk);
            _request.ClientAssertion = new()
            {
                Type = type,
                Value = clientAssertion
            };

            return this;
        }

        /// <summary>
        /// Add scope to ClientCredentialsTokenRequest
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public ClientCredentialRequestBuilder WithScope(string scope)
        {
            _request.Scope = scope;

            return this;
        }

        /// <summary>
        ///  Build the ClientCredentialsTokenRequest
        /// </summary>
        /// <returns></returns>
        public ClientCredentialsTokenRequest Build() => _request;
    }
}
