namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    /// <summary>
    /// Response after updating client secret
    /// </summary>
    public class ClientSecretUpdateResponse
    {
        /// <summary>
        /// Expirationdate string of the newly updated keys
        /// </summary>
        public required string ExpirationDate { get; set; }
        /// <summary>
        /// ClientId of the client who has had their keys updated
        /// </summary>
        public required string ClientId { get; set; }
        /// <summary>
        /// The new public key id of the updated pair
        /// </summary>
        public required string NewKeyId { get; set; }
    }
}
