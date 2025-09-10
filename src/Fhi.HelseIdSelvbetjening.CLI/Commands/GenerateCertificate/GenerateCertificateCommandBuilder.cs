using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.GenerateCertificate
{
    internal class GenerateCertificateCommandBuilder : ICommandBuilder
    {
        private readonly GenerateCertificateCommandHandler _commandHandler;

        public GenerateCertificateCommandBuilder(GenerateCertificateCommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }
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
            
            generateCertCommand.Handler = CommandHandler.Create(async (string certificateCommonName, string certificatePassword, string? certificateDirectory) =>
            {
                return await _commandHandler.ExecuteAsync(
                    certificateCommonName, 
                    certificatePassword, 
                    certificateDirectory
                );
            });
            
            return generateCertCommand;
        }
    }
}