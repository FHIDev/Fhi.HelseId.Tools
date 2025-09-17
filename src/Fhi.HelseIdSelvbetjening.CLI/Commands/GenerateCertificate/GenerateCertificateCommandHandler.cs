using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateCertificate
{
    internal class GenerateCertificateCommandHandler(
        IFileHandler fileHandler,
        ILogger<GenerateCertificateCommandHandler> logger)
    {
        private readonly IFileHandler _fileHandler = fileHandler;
        private readonly ILogger<GenerateCertificateCommandHandler> _logger = logger;

        /// <summary>
        /// Generates private and public key in certificate format.
        /// Stores in executing directory if path not specified.
        /// Public Key will be named CommonName_public.pem
        /// Password protected privateKey will be named CommonName_private.pfx
        /// Certificate thumbprint will be stored to CommonName_thumbprint.txt
        /// </summary>
        /// <returns></returns>
        public void Execute(GenerateCertificateParameters parameters)
        {
            using (_logger.BeginScope("CertificateCommonName: {CertificateCommonName}", parameters.CertificateCommonName))
            {
                var certPath = parameters.CertificateDirectory ?? Environment.CurrentDirectory;
                if (!_fileHandler.PathExists(certPath))
                {
                    _logger.LogInformation("Certificate path did not exist. Creating folder {@CertPath}", certPath);
                    _fileHandler.CreateDirectory(certPath);
                }

                CertificateFiles certificateFiles = GenerateCertificates(parameters.CertificateCommonName, parameters.CertificatePassword);

                var privateCertPath = Path.Combine(certPath, $"{parameters.CertificateCommonName}_private.pfx");
                var publicCertPath = Path.Combine(certPath, $"{parameters.CertificateCommonName}_public.pem");
                var thumbprintPath = Path.Combine(certPath, $"{parameters.CertificateCommonName}_thumbprint.txt");

                _fileHandler.WriteAllBytes(privateCertPath, certificateFiles.CertificatePrivateKey);
                _fileHandler.WriteAllText(publicCertPath, certificateFiles.CertificatePublicKey);
                _fileHandler.WriteAllText(thumbprintPath, certificateFiles.CertificateThumbprint);

                _logger.LogInformation("Private certificate saved: {@Path}", privateCertPath);
                _logger.LogInformation("Public certificate saved: {@Path}", publicCertPath);
                _logger.LogInformation("Thumbprint saved: {@Path}", thumbprintPath);
            }
        }

        // Move into Fhi.Authentcation?
        private CertificateFiles GenerateCertificates(string commonName, string password)
        {
            using var rsa = RSA.Create(2048); // Or 4096?
            var request = new CertificateRequest(
                $"CN={commonName}",
                rsa,
                HashAlgorithmName.SHA256,   // Or 512?
                RSASignaturePadding.Pkcs1);

            // Evaluate certificate validity period!
            var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
            
            var privateKeyBytes = cert.Export(X509ContentType.Pfx, password);
            
            var publicKeyBytes = cert.Export(X509ContentType.Cert);
            var publicKeyBase64 = Convert.ToBase64String(publicKeyBytes, Base64FormattingOptions.InsertLineBreaks);
            var publicKey = $"-----BEGIN CERTIFICATE-----\n{publicKeyBase64}\n-----END CERTIFICATE-----";
            
            var thumbprint = cert.Thumbprint;
            Console.WriteLine($"Certificate thumbprint: {thumbprint}");

            return new CertificateFiles
            {
                CertificatePrivateKey = privateKeyBytes,
                CertificatePublicKey = publicKey,
                CertificateThumbprint = thumbprint
            };
        }
    }
}
