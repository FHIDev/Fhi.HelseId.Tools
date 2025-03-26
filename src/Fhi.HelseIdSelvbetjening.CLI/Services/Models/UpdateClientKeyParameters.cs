
/// <summary>
/// Parameters used when updating a client secret (jwk's)
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
    public string? ExisitingPrivateJwk { get; set; }
    /// <summary>
    /// Path to the new public generated client secret (jwk). Will use <NewKey></NewKey> first.
    /// </summary>
    public string? NewPublicJwkPath { get; set; }
    /// <summary>
    /// The Clients new Jwk
    /// </summary>
    public string? NewPublicJwk { get; set; }
};
