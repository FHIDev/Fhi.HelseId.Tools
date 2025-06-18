using System.Text.Json.Serialization;

namespace Fhi.HelseIdSelvbetjening.CLI.Infrastructure
{
    /// <summary>
    /// Configuration options for the global exception handler
    /// </summary>
    public class GlobalExceptionHandlerOptions
    {
        /// <summary>
        /// Whether to include stack traces in user error messages (for debugging)
        /// </summary>
        public bool IncludeStackTraceInUserMessage { get; set; } = false;

        /// <summary>
        /// Whether to include correlation IDs in user error messages
        /// </summary>
        public bool IncludeCorrelationIdInUserMessage { get; set; } = false;

        /// <summary>
        /// Whether to log command parameters (may contain sensitive data)
        /// </summary>
        public bool LogCommandParameters { get; set; } = true;

        /// <summary>
        /// Maximum length of parameter values to log (prevents excessive logging)
        /// </summary>
        public int MaxParameterLogLength { get; set; } = 100;

        /// <summary>
        /// Custom exit code mappings for specific exception types
        /// </summary>
        public Dictionary<string, int> CustomExitCodes { get; set; } = new();

        /// <summary>
        /// Custom error message templates for specific exception types
        /// </summary>
        public Dictionary<string, string> CustomErrorMessages { get; set; } = new();
    }
}
