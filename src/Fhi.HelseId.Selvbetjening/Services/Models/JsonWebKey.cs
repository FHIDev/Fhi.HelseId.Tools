namespace Fhi.HelseIdSelvbetjening.Services.Models
{
    /// <summary>
    /// Represents a JWK for parsing kid property
    /// </summary>
    public class JsonWebKey
    {
        /// <summary>
        /// Key identifier
        /// </summary>
        public string? Kid { get; set; }

        /// <summary>
        /// Key type (e.g., "RSA")
        /// </summary>
        public string? Kty { get; set; }

        /// <summary>
        /// Private key component (for RSA keys)
        /// </summary>
        public string? D { get; set; }

        /// <summary>
        /// Modulus (for RSA keys)
        /// </summary>
        public string? N { get; set; }

        /// <summary>
        /// Exponent (for RSA keys)
        /// </summary>
        public string? E { get; set; }
    }
}
