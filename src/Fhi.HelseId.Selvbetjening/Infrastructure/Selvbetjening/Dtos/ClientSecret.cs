namespace Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening.Dtos
{
    /// <summary>
    /// Represents a client secret from the HelseID API
    /// </summary>
    internal class ClientSecret
    {
        /// <summary>
        /// The expiration date of the client secret
        /// </summary>
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// The key identifier
        /// </summary>
        public string? Kid { get; set; }

        /// <summary>
        /// The JWK thumbprint
        /// </summary>
        public string? JwkThumbprint { get; set; }

        /// <summary>
        /// The origin of the key (e.g., "Api", "Gui")
        /// </summary>
        public string? Origin { get; set; }

        /// <summary>
        /// The public JWK (usually null in responses)
        /// </summary>
        public string? PublicJwk { get; set; }
    }
}
