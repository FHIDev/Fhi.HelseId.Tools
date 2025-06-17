using System.Net;

namespace Fhi.HelseIdSelvbetjening.Services.Models
{
    /// <summary>
    /// Response containing client secret expiration information
    /// </summary>
    /// <param name="HttpStatus">Status code</param>
    /// <param name="Message">Message</param>
    /// <param name="ExpirationDate">The expiration date of the client secret</param>
    /// <param name="ValidationErrors">List of validation errors, if any</param>
    public record ClientSecretExpirationResponse(
        HttpStatusCode HttpStatus,
        string? Message,
        DateTime ExpirationDate = default,
        IReadOnlyList<string>? ValidationErrors = null)
    {
        /// <summary>
        /// Gets whether this response represents a validation failure
        /// </summary>
        public bool HasValidationErrors => ValidationErrors?.Any() == true;
        /// <summary>
        /// Creates a response for validation errors
        /// </summary>
        /// <param name="validationResult">The validation result containing errors</param>
        /// <returns>A response indicating validation failure</returns>
        public static ClientSecretExpirationResponse FromValidationErrors(ValidationResult validationResult)
        {
            return new ClientSecretExpirationResponse(
                HttpStatusCode.BadRequest,
                "Validation failed",
                DateTime.MinValue,
                validationResult.Errors);
        }
    };
}
