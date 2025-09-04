using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions
{
    internal static class KeyResolutionExtensions
    {
        public static string ResolveKey(
            string? directValue,
            string? filePath,
            string keyLabel,
            ILogger logger,
            IFileHandler fileHandler)
        {
            if (!string.IsNullOrWhiteSpace(directValue))
            {
                logger.LogInformation("{keyLabel} provided directly.", keyLabel);
                return directValue;
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                logger.LogInformation("{keyLabel} loaded from file: {filePath}", keyLabel, filePath);
                return fileHandler.ReadAllText(filePath);
            }

            logger.LogWarning("{keyLabel} not provided.", keyLabel);
            return string.Empty;
        }
    }
}