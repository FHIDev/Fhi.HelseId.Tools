using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateCertificate
{
    internal class GenerateCertificateCommandHandler
    {
        private readonly IFileHandler _fileHandler;
        private readonly ILogger<GenerateCertificateCommandHandler> _logger;

        public GenerateCertificateCommandHandler(
            IFileHandler fileHandler, 
            ILogger<GenerateCertificateCommandHandler> logger)
        {
            _fileHandler = fileHandler;
            _logger = logger;
        }

        /// <summary>
        /// Generates private and public key in certificate format.
        /// Stores in executing directory if path not specified.
        /// Public Key will be named CommonName_public.pem
        /// Password protected privateKey will be named CommonName_private.pfx
        /// Certificate thumbprint will be stored to CommonName_thumbprint.txt
        /// </summary>
        /// <returns></returns>
        public Task<int> ExecuteAsync(string commonName, string password, string? directory)
        {
            try
            {
                using (_logger.BeginScope("CertificateCommonName: {CertificateCommonName}", commonName))
                {
                    var certPath = directory ?? Environment.CurrentDirectory;
                    if (!_fileHandler.PathExists(certPath))
                    {
                        _logger.LogInformation("Certificate path did not exist. Creating folder {@CertPath}", certPath);
                        _fileHandler.CreateDirectory(certPath);
                    }

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        _logger.LogError("Required option '--CertificatePassword' is missing.");
                        throw new ArgumentException("Required option '--CertificatePassword' is missing.");
                    }

                    CertificateFiles certificateFiles = GenerateCertificates(commonName, password);

                    var privateCertPath = Path.Combine(certPath, $"{commonName}_private.pfx");
                    var publicCertPath = Path.Combine(certPath, $"{commonName}_public.pem");
                    var thumbprintPath = Path.Combine(certPath, $"{commonName}_thumbprint.txt");

                    _fileHandler.WriteAllBytes(privateCertPath, certificateFiles.CertificatePrivateKey);
                    _fileHandler.WriteAllText(publicCertPath, certificateFiles.CertificatePublicKey);
                    _fileHandler.WriteAllText(thumbprintPath, certificateFiles.CertificateThumbprint);

                    _logger.LogInformation("Private certificate saved: {@Path}", privateCertPath);
                    _logger.LogInformation("Public certificate saved: {@Path}", publicCertPath);
                    _logger.LogInformation("Thumbprint saved: {@Path}", thumbprintPath);
                    return Task.FromResult(0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating certificate: {Message}", ex.Message);
                return Task.FromResult(1);
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
