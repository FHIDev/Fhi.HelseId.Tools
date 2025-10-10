using System.Net;

namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    /// <summary>
    /// TODO: improve error result design. e.g. use error messages, types (warning,error, info etc.) error codes, etc.
    /// </summary>
    public class ErrorResult
    {
        public IEnumerable<IErrorMessage> Errors => _errors;
        private readonly List<IErrorMessage> _errors = [];
        /// <summary>
        /// Gets whether the validation was successful (no errors)
        /// </summary>
        public bool IsValid => !Errors.Any();

        public interface IErrorMessage { 
            string ErrorMessageText { get; }
            HttpStatusCode HttpStatusCode { get; }
            string ErrorType { get; }
        };

        public record ErrorMessage(
            string ErrorMessageText,
            HttpStatusCode HttpStatusCode,
            string ErrorType
        ) : IErrorMessage;

        /// <summary>
        /// Adds a validation error
        /// </summary>
        /// <param name="error">The error message to add</param>
        public void AddError(IErrorMessage error)
        {
            if (!string.IsNullOrWhiteSpace(error.ErrorMessageText))
            {
                _errors.Add(error);
            }
        }

        /// <summary>
        /// Adds multiple validation errors
        /// </summary>
        /// <param name="errors">The error messages to add</param>
        public void AddErrors(IEnumerable<IErrorMessage> errors)
        {
            foreach (var error in errors.Where(e => !string.IsNullOrWhiteSpace(e.ErrorMessageText)))
            {
                _errors.Add(error);
            }
        }
    }
}
