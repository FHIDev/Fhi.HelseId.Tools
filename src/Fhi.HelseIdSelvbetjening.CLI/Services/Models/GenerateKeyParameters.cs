
/// <summary>
/// Parameters for generating Client private and public Json web keys
/// </summary>
internal class GenerateKeyParameters
{
    /// <summary>
    /// Name of the file where keys will be stored
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Path of the file where keys will be stored
    /// </summary>
    public string? KeyPath { get; set; }
};
