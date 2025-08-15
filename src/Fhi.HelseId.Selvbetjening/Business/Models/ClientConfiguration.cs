namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    /// <summary>
    /// HelseId client
    /// </summary>
    /// <param name="ClientId">The client identifier</param>
    /// <param name="Jwk">The private JWK key for the client</param>
    public record ClientConfiguration(string ClientId, string Jwk);
}
