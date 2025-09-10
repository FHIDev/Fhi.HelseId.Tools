using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateCertificate
{
    internal class CertificateGeneratorService
    {
        private readonly GenerateCertificateParameters _parameters;
        private readonly IFileHandler _fileHandler;
        private readonly ILogger<CertificateGeneratorService> _logger;

        public CertificateGeneratorService(GenerateCertificateParameters parameters, IFileHandler fileHandler, ILogger<CertificateGeneratorService> logger)
        {
            _parameters = parameters;
            _fileHandler = fileHandler;
            _logger = logger;
        }

        /// <summary>
        /// Generates private and public key in certificate format.
        /// Stores in executing directory if path not specified.
        /// Public Key will be named CommonName_public.pem
        /// Password protected privateKey will be named CommonName_private.pfx
        /// </summary>
        /// <returns></returns>
        public Task ExecuteAsync()
        {
            var certPath = _parameters.CertificateDirectory ?? Environment.CurrentDirectory;
            if (!_fileHandler.PathExists(certPath))
            {
                _logger.LogInformation("Certificate path did not exist. Creating folder {@CertPath}", certPath);
                _fileHandler.CreateDirectory(certPath);
            }
            
            var password = _parameters.CertificatePassword;
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError("Required option '--CertificatePassword' is missing.");  
                throw new ArgumentException("Required option '--CertificatePassword' is missing.");
            }
            
            CertificateFiles certificateFiles = GenerateCertificates();

            var privateCertPath = Path.Combine(certPath, $"{_parameters.CertificateCommonName}_private.pfx");
            var publicCertPath = Path.Combine(certPath, $"{_parameters.CertificateCommonName}_public.pem");
            var thumbprintPath = Path.Combine(certPath, $"{_parameters.CertificateCommonName}_thumbprint.txt");
            
            _fileHandler.WriteAllBytes(privateCertPath, certificateFiles.CertificatePrivateKey);
            _fileHandler.WriteAllText(publicCertPath, certificateFiles.CertificatePublicKey);
            _fileHandler.WriteAllText(thumbprintPath, certificateFiles.CertificateThumbprint);

            _logger.LogInformation("Private certificate saved: {@Path}", privateCertPath);
            _logger.LogInformation("Public certificate saved: {@Path}", publicCertPath);
            _logger.LogInformation("Thumbprint saved: {@Path}", thumbprintPath);

            return Task.CompletedTask;
        }

        // Move into Fhi.Authentcation?
        private CertificateFiles GenerateCertificates()
        {
            using var rsa = RSA.Create(2048); // Or 4096?
            var request = new CertificateRequest(
                $"CN={_parameters.CertificateCommonName}",
                rsa,
                HashAlgorithmName.SHA256,   // Or 512?
                RSASignaturePadding.Pkcs1);

            // Evaluate certificate validity period!
            var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
            
            var privateKeyBytes = cert.Export(X509ContentType.Pfx, _parameters.CertificatePassword);
            
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
