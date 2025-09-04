namespace Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey
{
    public record UpdateClientKeyOptionNames(string Long, string Short);

    internal static class UpdateClientKeyParameterNames
    {
        public const string CommandName = "updateclientkey";
        public static readonly UpdateClientKeyOptionNames ClientId = new("ClientId", "c");
        public static readonly UpdateClientKeyOptionNames ExistingPrivateJwkPath = new("ExistingPrivateJwkPath", "ep");
        public static readonly UpdateClientKeyOptionNames ExistingPrivateJwk = new("ExistingPrivateJwk", "e");
        public static readonly UpdateClientKeyOptionNames NewPublicJwkPath = new("NewPublicJwkPath", "np");
        public static readonly UpdateClientKeyOptionNames NewPublicJwk = new("NewPublicJwk", "n");
        public static readonly UpdateClientKeyOptionNames YesOption = new("Yes", "y");
        public static readonly UpdateClientKeyOptionNames AuthorityUrl = new("AuthorityUrl", "a");
        public static readonly UpdateClientKeyOptionNames BaseAddress = new("BaseAddress", "b");
    }


    /// <summary>
    /// Parameters used when updating a client secret (JWK's)
    /// </summary>
    internal class UpdateClientKeyParameters
    {
        /// <summary>
        /// The client identifier for the Client that should be updated
        /// </summary>
        public required string ClientId { get; set; }
        /// <summary>
        /// Path to the existing client secret (jwk). Will use <OldKey></OldKey> first.
        /// </summary>
        public string? ExistingPrivateJwkPath { get; set; }
        /// <summary>
        /// The Clients existing client secret (private Jwk)
        /// </summary>
        public string? ExistingPrivateJwk { get; set; }
        /// <summary>
        /// Path to the new public generated client secret (JWK). Will use <NewKey></NewKey> first.
        /// </summary>
        public string? NewPublicJwkPath { get; set; }
        /// <summary>
        /// The Clients new Jwk
        /// </summary>
        public string? NewPublicJwk { get; set; }
        /// <summary>
        /// Url of athority
        /// </summary>
        public required string AuthorityUrl { get; set; }
        /// <summary>
        /// Baseaddress of target API
        /// </summary>
        public required string BaseAddress { get; set; }

        public bool Yes { get; set; }
    };
}