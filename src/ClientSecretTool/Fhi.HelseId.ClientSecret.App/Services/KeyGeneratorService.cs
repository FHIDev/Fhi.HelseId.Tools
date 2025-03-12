using Fhi.Cryptographic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Program;

namespace Fhi.HelseId.ClientSecret.App.Services
{
    internal class KeyGeneratorService : IHostedService
    {
        private readonly GenerateKeyParameters _parameters;
        private readonly ILogger<KeyGeneratorService> _logger;

        public KeyGeneratorService(GenerateKeyParameters parameters, ILogger<KeyGeneratorService> logger)
        {
            _parameters = parameters;
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
                Console.WriteLine($"Key path did not exist. Creating folder {keyPath}");
                Directory.CreateDirectory(keyPath);
            }

            var keyPair = JwkGenerator.GenerateRsaJwk();

            string privateKeyPath = Path.Combine(keyPath, $"{_parameters.FileName}_private.json");
            string publicKeyPath = Path.Combine(keyPath, $"{_parameters.FileName}_public.json");

            File.WriteAllText(privateKeyPath, keyPair.privateKey);
            File.WriteAllText(publicKeyPath, keyPair.publicKey);

            Console.WriteLine($"Private key saved: {privateKeyPath}");
            Console.WriteLine($"Public key saved: {publicKeyPath}");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
