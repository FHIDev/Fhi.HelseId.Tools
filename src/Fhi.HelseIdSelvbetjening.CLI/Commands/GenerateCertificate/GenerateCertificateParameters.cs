namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateCertificate
{
    public record UpdateGenerateCertificateOptionNames(string Long, string Short);

    internal static class GenerateCertificateParameterNames
    {
        public const string CommandName = "generatecertificate";
        public static readonly UpdateGenerateCertificateOptionNames CertificateCommonName = new("CertificateCommonName", "cn");
        public static readonly UpdateGenerateCertificateOptionNames CertificatePassword = new("CertificatePassword", "pwd");
        public static readonly UpdateGenerateCertificateOptionNames CertificateDirectory = new("CertificateDirectory", "dir");
    }

    /// <summary>
    /// Parameters for generating exportable certificates with RSA algorithm with 2048-bit key length
    /// </summary>
    internal class GenerateCertificateParameters
    {
        /// <summary>
        /// Common name of certificates
        /// </summary>
        public required string CertificateCommonName { get; set; }

        /// <summary>
        /// Password for the certificate
        /// </summary>
        public required string CertificatePassword { get; set; }

        /// <summary>
        /// Directory where public and private certificates will be stored
        /// </summary>
        public string? CertificateDirectory { get; set; }
    };
}