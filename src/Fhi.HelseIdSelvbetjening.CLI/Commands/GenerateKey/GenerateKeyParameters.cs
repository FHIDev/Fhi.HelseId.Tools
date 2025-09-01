namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey
{
    public record UpdateGenerateKeyOptionNames(string Long, string Short);

    internal static class GenerateKeyParameterNames
    {
        public const string CommandName = "generatekey";
        public static readonly UpdateGenerateKeyOptionNames KeyFileNamePrefix = new("KeyFileNamePrefix", "n");
        public static readonly UpdateGenerateKeyOptionNames KeyDirectory = new("KeyDirectory", "d");
    }

    /// <summary>
    /// Parameters for generating Client private and public Json web keys
    /// </summary>
    internal class GenerateKeyParameters
    {
        /// <summary>
        /// Prefix name of the file
        /// </summary>
        public string? KeyFileNamePrefix { get; set; }

        /// <summary>
        /// Directory where public and private file will be stored
        /// </summary>
        public string? KeyDirectory { get; set; }
    };
}