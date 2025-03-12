using Fhi.HelseId.Selvbetjening.Models;

namespace Fhi.HelseId.Selvbetjening
{
    public interface IHelseIdSelvbetjeningService
    {
        public Task<ClientSecretUpdateResponse> UpdateClientSecret(ClientConfiguration clientToUpdate, string newPublicJwk);
        public Task<ClientSecretStatusResponse> GetClientSecretStatus(ClientConfiguration clientToUpdate);
    }
}
