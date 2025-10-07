namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    /// <summary>
    /// Response after updating client secret
    /// </summary>
    public class ClientSecretUpdateResponse
    {
        public required string ExpirationDate { get; set; }
        public string? ClientId { get; set; }
    }
}
