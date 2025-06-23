namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Validates that a string represents a valid Unix epoch timestamp.
        /// Unix epoch timestamps are typically 10 digits long for dates from 2001-2286.
        /// This validates that the string is a reasonable epoch timestamp (not too far in past/future).
        /// </summary>
        /// <param name="value">The string to validate as an epoch timestamp</param>
        /// <returns>True if the string is a valid epoch timestamp, false otherwise</returns>
        internal static bool IsValidEpochTimestamp(this string value)
        {
            // Check if it's a valid 10-digit epoch timestamp
            if (!long.TryParse(value, out long epochSeconds))
                return false;

            // Reasonable range: January 1, 2000 (946684800) to December 31, 2099 (4102444799)
            // This avoids both very old dates and far future dates that would indicate an error
            return epochSeconds >= 946684800 && epochSeconds <= 4102444799;
        }
    }
}
