namespace Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration
{
    public record ReadClientSecretExpirationOptionNames(string Long, string Short);

    public static class ReadClientSecretExpirationParameterNames
    {
        public const string CommandName = "readclientsecretexpiration";
        public static readonly ReadClientSecretExpirationOptionNames ClientId = new("ClientId", "c");
        public static readonly ReadClientSecretExpirationOptionNames ExistingPrivateJwkPath = new("ExistingPrivateJwkPath", "ep");
        public static readonly ReadClientSecretExpirationOptionNames ExistingPrivateJwk = new("ExistingPrivateJwk", "e");
    }

    /// <summary>
    /// Parameters used when reading client secret expiration
    /// </summary>
    internal class ReadClientSecretExpirationParameters
    {
        /// <summary>
        /// The client identifier for the Client that should be queried
        /// </summary>
        public required string ClientId { get; set; }
        /// <summary>
        /// Path to the existing client secret (jwk).
        /// </summary>
        public string? ExistingPrivateJwkPath { get; set; }
        /// <summary>
        /// The Clients existing client secret (private Jwk)
        /// </summary>
        public string? ExistingPrivateJwk { get; set; }
    }
}
