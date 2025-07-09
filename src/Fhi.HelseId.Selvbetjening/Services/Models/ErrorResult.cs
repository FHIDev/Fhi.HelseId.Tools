namespace Fhi.HelseIdSelvbetjening.Services.Models
{
    /// <summary>
    /// TODO: improve error result design. e.g. use error messages, types (warning,error, info etc.) error codes, etc.
    /// </summary>
    public class ErrorResult
    {
        public readonly List<string> Errors = new();

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
                Errors.Add(error);
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
                Errors.Add(error);
            }
        }
    }
}
