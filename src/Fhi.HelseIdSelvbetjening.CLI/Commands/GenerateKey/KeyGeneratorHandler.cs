using Fhi.Authentication.Tokens;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateKey
{
    internal class KeyGeneratorHandler
    {
        private readonly GenerateKeyParameters _parameters;
        private readonly IFileHandler _fileHandler;
        private readonly ILogger<KeyGeneratorHandler> _logger;

        public KeyGeneratorHandler(GenerateKeyParameters parameters, IFileHandler fileHandler, ILogger<KeyGeneratorHandler> logger)
        {
            _parameters = parameters;
            _fileHandler = fileHandler;
            _logger = logger;
        }

        /// <summary>
        /// Generates private and public key.
        /// Stores in executing directory if path not specified.
        /// privateKey will be named FileName_private.json
        /// privateKey will be named FileName_public.json 
        /// </summary>
        /// <returns></returns>
        public Task ExecuteAsync()
        {
            var keyPath = _parameters.KeyDirectory ?? Environment.CurrentDirectory;
            if (!_fileHandler.PathExists(keyPath))
            {
                _logger.LogInformation("Key path did not exist. Creating folder {@KeyPath}", keyPath);
                _fileHandler.CreateDirectory(keyPath);
            }

            var keyPair = JwkGenerator.GenerateRsaJwk();

            var privateKeyPath = Path.Combine(keyPath, $"{_parameters.KeyFileNamePrefix}_private.json");
            var publicKeyPath = Path.Combine(keyPath, $"{_parameters.KeyFileNamePrefix}_public.json");

            _fileHandler.WriteAllText(privateKeyPath, keyPair.PrivateKey);
            _fileHandler.WriteAllText(publicKeyPath, keyPair.PublicKey);

            _logger.LogInformation("Private key saved: {@PrivateKeyPath}", privateKeyPath);
            _logger.LogInformation("Public key saved: {@PublicKeyPath}", publicKeyPath);

            return Task.CompletedTask;
        }
    }
}
