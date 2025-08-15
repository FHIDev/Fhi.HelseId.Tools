
using Fhi.HelseIdSelvbetjening.Business.Models;

namespace Fhi.HelseIdSelvbetjening.Business
{
    /// <summary>
    /// Service for handling HelseId clients such as reading secret (key) expirations, updates client secrets
    /// </summary>
    public interface IHelseIdSelvbetjeningService
    {
        /// <summary>
        /// Add new (public jwk key) secret to an existing client 
        /// </summary>
        /// <param name="clientToUpdate">The client that should be updated</param>
        /// <param name="newPublicJwk">New public key for client</param>
        /// <returns></returns>
        public Task<ClientSecretUpdateResponse> UpdateClientSecret(ClientConfiguration clientToUpdate, string newPublicJwk);

        /// <summary>
        /// Read the expiration date of a client secret
        /// </summary>
        /// <param name="clientConfiguration">The client configuration</param>
        /// <returns>Response containing expiration information</returns>
        public Task<IResult<ClientSecretExpirationResponse, ErrorResult>> ReadClientSecretExpiration(ClientConfiguration clientConfiguration);
    }
}
