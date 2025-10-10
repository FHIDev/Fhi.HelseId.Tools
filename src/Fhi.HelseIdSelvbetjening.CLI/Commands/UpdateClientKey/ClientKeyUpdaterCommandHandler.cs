using Fhi.HelseIdSelvbetjening.Business;
using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.UpdateClientKey
{
    internal class ClientKeyUpdaterCommandHandler(
        IHostEnvironment hostEnvironment,
        IHelseIdSelvbetjeningService helseIdSelvbetjeningService,
        IFileHandler fileHandler,
        ILogger<ClientKeyUpdaterCommandHandler> logger)
    {
        public async Task<int> ExecuteAsync(UpdateClientKeyParameters parameters)
        {
            using (logger.BeginScope("Updating Keys for ClientId: {ClientId}", parameters.ClientId))
            {
                var environment = hostEnvironment.EnvironmentName;
                logger.LogInformation("Environment: {environment}", environment);

                if (!parameters.Yes)
                {
                    logger.LogInformation("Update client in environment {environment}? y/n", environment);
                    var input = Console.ReadLine();
                    if (input?.Trim().ToLower() != "y")
                    {
                        logger.LogInformation("Operation cancelled.");
                        return 0;
                    }
                }

                var newKey = KeyResolutionExtensions.ResolveKeyValuePathOrString(
                parameters.NewPublicJwk,
                parameters.NewPublicJwkPath,
                "New key",
                logger,
                fileHandler);

                var oldKey = KeyResolutionExtensions.ResolveKeyValuePathOrString(
                parameters.ExistingPrivateJwk,
                parameters.ExistingPrivateJwkPath,
                "Old key",
                logger,
                fileHandler);

                bool newKeyExists = !string.IsNullOrEmpty(newKey);
                bool oldKeyExists = !string.IsNullOrEmpty(oldKey);

                if (!newKeyExists || !oldKeyExists)
                {
                    logger.LogError("One or more parameters empty.");
                    logger.LogInformation("New key found: {newKeyExists} Old key found: {oldKeyExists}", newKeyExists, oldKeyExists);
                    return 1;
                }

                var result = await helseIdSelvbetjeningService.UpdateClientSecret(new ClientConfiguration(
                parameters.ClientId, oldKey),
                parameters.AuthorityUrl, parameters.BaseAddress, newKey);

                return result.HandleResponse(
                    onSuccess: value =>
                    {
                        logger.LogInformation("Keys successfully updated.");
                        logger.LogInformation("Updated keys for Client: {clientId}", value.ClientId);
                        logger.LogInformation("New public key Id: {keyId}", value.NewKeyId);
                        logger.LogInformation("Expiration Date: {resultMessage}", value.ExpirationDate);
                        return 0;
                    },
                    onError: (result) =>
                    {
                        var allMessages = string.Join("; ", result.Errors.Select(e => e.ErrorMessageText));
                        logger.LogError("Details: {Details}", allMessages);
                        return 1;
                    }
                );
            }
        }
    }
}