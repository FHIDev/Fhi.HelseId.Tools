namespace Fhi.HelseIdSelvbetjening.Services.Models
{
    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        private readonly List<string> _errors = new();

        /// <summary>
        /// Gets whether the validation was successful (no errors)
        /// </summary>
        public bool IsValid => !_errors.Any();

        /// <summary>
        /// Gets the list of validation errors
        /// </summary>
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

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

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public static ValidationResult Success() => new();

        /// <summary>
        /// Creates a validation result with a single error
        /// </summary>
        /// <param name="error">The error message</param>
        public static ValidationResult WithError(string error)
        {
            var result = new ValidationResult();
            result.AddError(error);
            return result;
        }

        /// <summary>
        /// Creates a validation result with multiple errors
        /// </summary>
        /// <param name="errors">The error messages</param>
        public static ValidationResult WithErrors(IEnumerable<string> errors)
        {
            var result = new ValidationResult();
            result.AddErrors(errors);
            return result;
        }
    }
}
