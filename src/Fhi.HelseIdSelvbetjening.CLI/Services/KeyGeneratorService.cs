using Fhi.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.Services
{
    internal class KeyGeneratorService : IHostedService
    {
        private readonly GenerateKeyParameters _parameters;
        private readonly IFileHandler _fileWriter;
        private readonly ILogger<KeyGeneratorService> _logger;

        public KeyGeneratorService(GenerateKeyParameters parameters, IFileHandler fileWriter, ILogger<KeyGeneratorService> logger)
        {
            _parameters = parameters;
            _fileWriter = fileWriter;
            _logger = logger;
        }

        /// <summary>
        /// Generates private and public key.
        /// Stores in executing directory if path not specified.
        /// privateKey will be named <FileName>_private.json
        /// privateKey will be named <FileName>_puplic.json
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var keyPath = _parameters.KeyPath ?? Environment.CurrentDirectory;
            if (!Directory.Exists(keyPath))
            {
                _logger.LogInformation("Key path did not exist. Creating folder {@KeyPath}", keyPath);
                Directory.CreateDirectory(keyPath);
            }

            var keyPair = JwkGenerator.GenerateRsaJwk();

            string privateKeyPath = Path.Combine(keyPath, $"{_parameters.FileName}_private.json");
            string publicKeyPath = Path.Combine(keyPath, $"{_parameters.FileName}_public.json");

            _fileWriter.WriteAllText(privateKeyPath, keyPair.PrivateKey);
            _fileWriter.WriteAllText(publicKeyPath, keyPair.PublicKey);

            _logger.LogInformation("Private key saved: {@PrivateKeyPath}", privateKeyPath);
            _logger.LogInformation("Public key saved: {@PublicKeyPath}", publicKeyPath);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
