
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
