namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    /// <summary>
    /// TODO: improve error result design. e.g. use error messages, types (warning,error, info etc.) error codes, etc.
    /// </summary>
    public class ErrorResult
    {
        public IEnumerable<string> Errors => _errors;
        private readonly List<string> _errors = [];
        /// <summary>
        /// Gets whether the validation was successful (no errors)
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Adds a validation error
        /// </summary>
        /// <param name="error">The error message to add</param>
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                _errors.Add(error);
            }
        }

        /// <summary>
        /// Adds multiple validation errors
        /// </summary>
        /// <param name="errors">The error messages to add</param>
        public void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                _errors.Add(error);
            }
        }
    }
}
