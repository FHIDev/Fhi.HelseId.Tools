using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fhi.HelseIdSelvbetjening.Services
{
    /// <summary>
    /// Provides shared JSON serialization configuration for consistent behavior across the application.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Creates standardized JsonSerializerOptions for HelseID services.
        /// Uses web defaults with camelCase naming policy and case-insensitive property matching.
        /// </summary>
        /// <returns>Configured JsonSerializerOptions</returns>
        public static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                IgnoreReadOnlyProperties = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }
}