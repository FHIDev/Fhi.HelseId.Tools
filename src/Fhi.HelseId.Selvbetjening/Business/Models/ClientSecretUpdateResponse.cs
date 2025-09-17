using System.Net;

namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    /// <summary>
    /// Response after updating client secret
    /// </summary>
    public class ClientSecretUpdateResponse
    {
        public HttpStatusCode? HttpStatus { get; set; }
        public string? Message { get; set; }
    }
}
