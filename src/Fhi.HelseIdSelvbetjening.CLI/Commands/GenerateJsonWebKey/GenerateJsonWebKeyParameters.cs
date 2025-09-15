namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateJsonWebKey
{
    public record UpdateGenerateJsonWebKeyOptionNames(string Long, string Short);

    internal static class GenerateJsonWebKeyParameterNames
    {
        public const string CommandName = "generatejsonwebkey";
        public static readonly UpdateGenerateJsonWebKeyOptionNames KeyFileNamePrefix = new("KeyFileNamePrefix", "n");
        public static readonly UpdateGenerateJsonWebKeyOptionNames KeyDirectory = new("KeyDirectory", "d");
    }

    /// <summary>
    /// Parameters for generating Client private and public Json web keys
    /// </summary>
    internal class GenerateJsonWebKeyParameters
    {
        /// <summary>
        /// Prefix name of the file
        /// </summary>
        public required string KeyFileNamePrefix { get; set; }

        /// <summary>
        /// Directory where public and private file will be stored
        /// </summary>
        public required string KeyDirectory { get; set; }
    };
}