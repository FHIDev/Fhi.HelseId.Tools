using Fhi.HelseIdSelvbetjening.Business;
using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening.Dtos;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Setup
{
    internal class HelseIdSelvbetjeningServiceBuilder
    {
        public ILogger<HelseIdSelvbetjeningService> Logger { get; private set; } = Substitute.For<ILogger<HelseIdSelvbetjeningService>>();

        public ITokenService TokenService { get; private set; } = Substitute.For<ITokenService>();

        public ISelvbetjeningApi SelvbetjeningApi { get; private set; } = Substitute.For<ISelvbetjeningApi>();

        public HelseIdSelvbetjeningServiceBuilder WithDPopTokenResponse(TokenResponse tokenResponse)
        {
            TokenService.RequestDPoPToken(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>())
                .Returns(Task.FromResult(tokenResponse));
            return this;
        }

        public HelseIdSelvbetjeningServiceBuilder WithUpdateClientSecretResponse(ClientSecretUpdateResult? updateResult = default!, ProblemDetail? problemDetail = null)
        {
            SelvbetjeningApi.UpdateClientSecretsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>())
                .Returns(Task.FromResult((updateResult, problemDetail)));

            return this;
        }

        public HelseIdSelvbetjeningServiceBuilder WithGetClientSecretResponse(IEnumerable<HelseIdSelvbetjening.Infrastructure.Selvbetjening.Dtos.ClientSecret>? clientSecrets = default!, ProblemDetail? problemDetail = null)
        {
            SelvbetjeningApi.GetClientSecretsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>())
                .Returns(Task.FromResult((clientSecrets, problemDetail)));

            return this;
        }

        public HelseIdSelvbetjeningService Build()
        {
            return new HelseIdSelvbetjeningService(
                TokenService,
                SelvbetjeningApi
            );
        }
    }
}
