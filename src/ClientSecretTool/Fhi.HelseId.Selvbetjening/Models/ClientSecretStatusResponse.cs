using System.Net;

namespace Fhi.HelseId.Selvbetjening.Models
{
    public record ClientSecretStatusResponse(HttpStatusCode HttpStatus, string Content);

    public record ClientSecretUpdateResponse(HttpStatusCode HttpStatus, string Content);

}
