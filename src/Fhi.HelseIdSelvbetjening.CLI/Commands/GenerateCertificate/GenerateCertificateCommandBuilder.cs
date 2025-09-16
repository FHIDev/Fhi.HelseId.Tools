using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;
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
            var generateCertCommand = new Command(
                GenerateCertificateParameterNames.CommandName,
                "Generate keys in PEM certificate format")
            {
                TreatUnmatchedTokensAsErrors = true
            };
            
            generateCertCommand.CreateStringOption(
                GenerateCertificateParameterNames.CertificateCommonName.Long,
                GenerateCertificateParameterNames.CertificateCommonName.Short,
                "Common Name (CN) for the certificate",
                isRequired: true);
            
            generateCertCommand.CreateStringOption(
                GenerateCertificateParameterNames.CertificatePassword.Long,
                GenerateCertificateParameterNames.CertificatePassword.Short,
                "Password for the generated certificate",
                isRequired: true);
            
            generateCertCommand.CreateStringOption(
                GenerateCertificateParameterNames.CertificateDirectory.Long,
                GenerateCertificateParameterNames.CertificateDirectory.Short,
                "Directory to store the generated certificates",
                isRequired: false);
            
            generateCertCommand.Handler = CommandHandler.Create((string certificateCommonName, string certificatePassword, string? certificateDirectory) =>
            {
                var parameters = new GenerateCertificateParameters
                {
                    CertificateCommonName = certificateCommonName,
                    CertificatePassword = certificatePassword,
                    CertificateDirectory = certificateDirectory
                };
                _commandHandler.Execute(parameters);
            });
            
            return generateCertCommand;
        }
    }
}