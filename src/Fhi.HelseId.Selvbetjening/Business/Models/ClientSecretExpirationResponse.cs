namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    /// <summary>
    /// Response containing client secret expiration information
    /// </summary>
    /// <param name="Origin"></param>
    /// <param name="ExpirationDate">The expiration date of the client secret</param>
    /// <param name="KeyId">The JWK key identifier</param>
    public record ClientSecret(
        DateTime? ExpirationDate,
        string? KeyId,
        string? Origin);

    public class ClientSecretExpirationResponse
    {
        /// <summary>
        /// The secret that matches the requested kid (if any).
        /// </summary>
        public ClientSecret? SelectedSecret { get; set; }

        /// <summary>
        /// All secrets returned by the API.
        /// </summary>
        public List<ClientSecret> AllSecrets { get; set; } = [];
    }
}
