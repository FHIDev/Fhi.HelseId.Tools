using System.Net;

namespace Fhi.HelseIdSelvbetjening.Services.Models
{
    /// <summary>
    /// Response containing client secret expiration information
    /// </summary>
    /// <param name="HttpStatus">Status code</param>
    /// <param name="Message">Message</param>
    /// <param name="ExpirationDate">The expiration date of the client secret, if available</param>
    public record ClientSecretExpirationResponse(HttpStatusCode HttpStatus, string? Message, DateTime? ExpirationDate = null);
}
