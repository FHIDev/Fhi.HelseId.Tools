namespace Fhi.HelseIdSelvbetjening.Services.Models
{
    /// <summary>
    /// Configuration options for HelseId Selvbetjening API
    /// </summary>
    public class SelvbetjeningConfiguration
    {
        /// <summary>
        /// HelseId authority
        /// </summary>
        public required string Authority { get; set; }
        /// <summary>
        /// Base address for HelseId Selvbetjening API
        /// </summary>
        public required string BaseAddress { get; set; }
        /// <summary>
        /// The HelseId Selvbetjening client secret endpoint used to read client secrets and update secrets
        /// </summary>
        public required string ClientSecretEndpoint { get; set; }
    }
}
