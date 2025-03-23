
public class UpdateClientKeyParameters
{
    public required string ClientId { get; set; }
    public string? OldKeyPath { get; set; }
    public string? OldKey { get; set; }

    public string? NewKeyPath { get; set; }
    public string? NewKey { get; set; }
};
