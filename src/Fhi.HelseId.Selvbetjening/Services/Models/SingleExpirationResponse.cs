namespace Fhi.HelseIdSelvbetjening.Services.Models
{
    /// <summary>
    /// Represents a single expiration response (non-array format)
    /// </summary>
    public class SingleExpirationResponse
    {
        /// <summary>
        /// The expiration date
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Alternative expiration property name
        /// </summary>
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// Unix timestamp expiration (alternative format)
        /// </summary>
        public long? Exp { get; set; }

        /// <summary>
        /// Gets the effective expiration date from any of the available formats
        /// </summary>
        public DateTime GetEffectiveExpirationDate()
        {
            // Try ExpirationDate first
            if (ExpirationDate.HasValue)
                return ExpirationDate.Value;

            // Try Expiration second
            if (Expiration.HasValue)
                return Expiration.Value;

            // Try Unix timestamp conversion last
            if (Exp.HasValue)
                return DateTimeOffset.FromUnixTimeSeconds(Exp.Value).DateTime;

            return DateTime.MinValue;
        }
    }
}
