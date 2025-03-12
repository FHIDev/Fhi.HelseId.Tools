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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var keyPath = _parameters.KeyPath ?? Environment.CurrentDirectory;
            if (!Directory.Exists(keyPath))
            {
                Console.WriteLine($"Key path did not exist. Creating folder {keyPath}");
                Directory.CreateDirectory(keyPath);
            }

            var keyPair = JwkGenerator.GenerateRsaJwk();

            string privateKeyPath = Path.Combine(keyPath, $"{_parameters.ClientId}_private.json");
            string publicKeyPath = Path.Combine(keyPath, $"{_parameters.ClientId}_public.json");

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
