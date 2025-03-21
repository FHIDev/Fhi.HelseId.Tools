using System.Net;

namespace Fhi.HelseId.Selvbetjening.Services.Models
{
    /// <summary>
    /// Response after updating client secret
    /// </summary>
    /// <param name="HttpStatus">Status code</param>
    /// <param name="Message">Message</param>
    public record ClientSecretUpdateResponse(HttpStatusCode HttpStatus, string? Message);
}
