using System.CommandLine;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateCertificate
{
    public class GenerateCertificateCommandBuilder : ICommandBuilder
    {
        public Command Build(IHost host)
        {
            var generateCertCommand = new Command(GenerateCertificateParameterNames.CommandName, "Generate keys in PEM certificate format");

            var certNameOption = new Option<string>([$"--{GenerateCertificateParameterNames.CertificateCommonName.Long}", $"-{GenerateCertificateParameterNames.CertificateCommonName.Short}"], "Common Name (CN) for the certificate") { IsRequired = true };
            generateCertCommand.AddOption(certNameOption);
            
            var certPasswordOption = new Option<string>([$"--{GenerateCertificateParameterNames.CertificatePassword.Long}", $"-{GenerateCertificateParameterNames.CertificatePassword.Short}"], "Password for the generated certificate") { IsRequired = true };
            generateCertCommand.AddOption(certPasswordOption);
            
            var certDirOption = new Option<string>([$"--{GenerateCertificateParameterNames.CertificateDirectory.Long}", $"-{GenerateCertificateParameterNames.CertificateDirectory.Short}"], "Directory to store the generated certificates");
            generateCertCommand.AddOption(certDirOption);

            generateCertCommand.TreatUnmatchedTokensAsErrors = true;
            generateCertCommand.SetHandler(
                async (certFileNamePrefix, certPassword, certDirectory) =>
                {
                    try
                    {
                        var parameters = new GenerateCertificateParameters
                        {
                            CertificateCommonName = certFileNamePrefix,
                            CertificatePassword = certPassword,
                            CertificateDirectory = certDirectory
                        };

                        var logger = host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CertificateGeneratorService>>();
                        var fileWriter = host.Services.GetRequiredService<IFileHandler>();
                        var service = new CertificateGeneratorService(parameters, fileWriter, logger);

                        await service.ExecuteAsync();
                    }
                    catch (Exception ex)
                    {
                        await Console.Error.WriteLineAsync($"Error generating key: {ex.Message}");
                        throw; // This will cause a non-zero exit code
                    }
                },
                certNameOption, certPasswordOption, certDirOption
            );

            return generateCertCommand;
        }
    }
}