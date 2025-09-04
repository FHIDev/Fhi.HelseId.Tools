namespace Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration
{
    public record ReadClientSecretExpirationOptionNames(string Long, string Short);

    public static class ReadClientSecretExpirationParameterNames
    {
        public const string CommandName = "readclientsecretexpiration";
        public static readonly ReadClientSecretExpirationOptionNames ClientId = new("ClientId", "c");
        public static readonly ReadClientSecretExpirationOptionNames ExistingPrivateJwkPath = new("ExistingPrivateJwkPath", "ep");
        public static readonly ReadClientSecretExpirationOptionNames ExistingPrivateJwk = new("ExistingPrivateJwk", "e");
        public static readonly ReadClientSecretExpirationOptionNames AuthorityUrl = new("AuthorityUrl", "a");
        public static readonly ReadClientSecretExpirationOptionNames BaseAddress = new("BaseAddress", "b");
    }

    internal class ReadClientSecretExpirationParameters
    {
        public required string ClientId { get; set; }
        public string? ExistingPrivateJwkPath { get; set; }
        public string? ExistingPrivateJwk { get; set; }
        public required string AuthorityUrl { get; set; }
        public required string BaseAddress { get; set; }
    }
}
