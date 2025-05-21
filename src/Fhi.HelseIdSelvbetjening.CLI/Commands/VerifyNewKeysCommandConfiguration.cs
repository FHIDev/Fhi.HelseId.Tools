using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Fhi.HelseIdSelvbetjening.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Fhi.HelseIdSelvbetjening.CLI.Services; // Ensure this using statement is present

namespace Fhi.HelseIdSelvbetjening.CLI.Commands
{
    public static class VerifyNewKeysCommandConfiguration
    {
        public static Command CreateCommand(IHost host, string commandName) // IHost can be removed if not used elsewhere in this method for setup
        {
            var verifyCommand = new Command(commandName, "Verify a new client secret with HelseID");
            verifyCommand.AddAlias("verify-new-keys");

            var clientIdOption = new Option<string>(
                aliases: ["--clientId", "-cid"],
                description: "Client ID to verify")
                { IsRequired = true };

            var privateJwkOption = new Option<string>(
                aliases: ["--privateJwk", "-jwk"], 
                description: "The private key in JWK format (as a JSON string).")
                { Arity = ArgumentArity.ZeroOrOne }; // Not required if path is used

            var privateJwkPathOption = new Option<string>(
                aliases: ["--privateJwkPath", "-path"],
                description: "Path to the file containing the private key in JWK format.")
                { Arity = ArgumentArity.ZeroOrOne }; // Not required if jwk string is used
            
            verifyCommand.AddOption(clientIdOption);
            verifyCommand.AddOption(privateJwkOption);
            verifyCommand.AddOption(privateJwkPathOption);

            verifyCommand.AddValidator(result =>
            {
                var jwkOptionResult = result.FindResultFor(privateJwkOption);
                var pathOptionResult = result.FindResultFor(privateJwkPathOption);
                if (jwkOptionResult != null && pathOptionResult != null)
                {
                    result.ErrorMessage = "Cannot use both --privateJwk and --privateJwkPath simultaneously.";
                }
                else if (jwkOptionResult == null && pathOptionResult == null)
                {
                    result.ErrorMessage = "Either --privateJwk or --privateJwkPath must be specified.";
                }
            });

            // Capture the 'host' instance from the CreateCommand method's parameter for the handler.
            verifyCommand.Handler = CommandHandler.Create<string, string?, string?>(
                (clientId, privateJwk, privateJwkPath) => 
                    HandleVerifyNewKeysCommand(clientId, privateJwk, privateJwkPath, host)); // 'host' here is from the outer scope
            
            return verifyCommand;
        }

        private static async Task HandleVerifyNewKeysCommand(string clientId, string? privateJwk, string? privateJwkPath, IHost host) // This 'host' will now be the captured one
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Handling verify-new-keys command...");
            logger.LogInformation("Received clientId: {ClientId}", clientId);
            logger.LogInformation("Received privateJwk (raw string length): {Length}", privateJwk?.Length ?? 0);
            logger.LogInformation("Received privateJwkPath: {Path}", privateJwkPath);

            string? jwkToVerify = null;

            if (!string.IsNullOrEmpty(privateJwkPath))
            {
                if (File.Exists(privateJwkPath))
                {
                    jwkToVerify = await File.ReadAllTextAsync(privateJwkPath);
                    logger.LogInformation("Read JWK from path {Path}. Length: {Length}", privateJwkPath, jwkToVerify.Length);
                }
                else
                {
                    logger.LogError("Private JWK file not found at path: {Path}", privateJwkPath);
                    Environment.ExitCode = 1;
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(privateJwk))
            {
                try
                {
                    byte[] data = Convert.FromBase64String(privateJwk);
                    jwkToVerify = System.Text.Encoding.UTF8.GetString(data);
                    logger.LogInformation("Successfully Base64 decoded privateJwk. Decoded length: {Length}", jwkToVerify.Length);
                }
                catch (FormatException)
                {
                    logger.LogWarning("Received privateJwk is not a valid Base64 string. Assuming it's plain JSON.");
                    jwkToVerify = privateJwk; 
                }
            }

            if (string.IsNullOrEmpty(jwkToVerify))
            {
                logger.LogError("No private JWK to verify. Ensure --privateJwk or --privateJwkPath is provided correctly.");
                Environment.ExitCode = 1;
                return;
            }

            using var scope = host.Services.CreateScope();
            var verifyService = scope.ServiceProvider.GetRequiredService<IVerifyNewKeysService>();

            logger.LogInformation("Calling verifyService.ExecuteAsync...");
            var success = await verifyService.ExecuteAsync(clientId, jwkToVerify);
            logger.LogInformation("verifyService.ExecuteAsync completed. Success: {SuccessState}", success);

            if (!success)
            {
                Environment.ExitCode = 1;
            }
        }
    }
}
