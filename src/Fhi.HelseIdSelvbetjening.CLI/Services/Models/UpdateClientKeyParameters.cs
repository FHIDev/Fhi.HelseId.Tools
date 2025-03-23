
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
    public string? OldClientJwkPath { get; set; }
    /// <summary>
    /// The Clients existing Jwk
    /// </summary>
    public string? OldClientJwk { get; set; }
    /// <summary>
    /// Path to the new generated client secret (jwk). Will use <NewKey></NewKey> first.
    /// </summary>
    public string? NewClientJwkPath { get; set; }
    /// <summary>
    /// The Clients new Jwk
    /// </summary>
    public string? NewClientJwk { get; set; }
};
