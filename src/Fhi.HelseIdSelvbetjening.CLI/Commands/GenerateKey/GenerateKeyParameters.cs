namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey
{

    internal static class GenerateKeyParameterNames
    {
        public const string CommandName = "generatekey";
        public const string KeyFileNamePrefixLong = "keyFileNamePrefix";
        public const string KeyFileNamePrefixShort = "n";
        public const string KeyDirectoryLong = "keyDirectory";
        public const string KeyDirectoryShort = "d";

        public static readonly CliOptionNames KeyFileNamePrefix = new(KeyFileNamePrefixLong, KeyFileNamePrefixShort);
        public static readonly CliOptionNames KeyDirectory = new(KeyDirectoryLong, KeyDirectoryShort);
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