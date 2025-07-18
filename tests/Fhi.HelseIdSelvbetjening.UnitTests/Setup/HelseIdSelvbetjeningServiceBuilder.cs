using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.Infrastructure.Dtos;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Setup
{
    internal class HelseIdSelvbetjeningServiceBuilder
    {
        public ILogger<HelseIdSelvbetjeningService> Logger { get; private set; } = Substitute.For<ILogger<HelseIdSelvbetjeningService>>();

        public Microsoft.Extensions.Options.IOptions<SelvbetjeningConfiguration> Options { get; private set; }
            = Substitute.For<Microsoft.Extensions.Options.IOptions<SelvbetjeningConfiguration>>();
        public ITokenService TokenService { get; private set; } = Substitute.For<ITokenService>();

        public ISelvbetjeningApi SelvbetjeningApi { get; private set; } = Substitute.For<ISelvbetjeningApi>();

        public HelseIdSelvbetjeningServiceBuilder WithDPopTokenResponse(TokenResponse tokenResponse)
        {
            TokenService.CreateDPoPToken(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>())
                .Returns(Task.FromResult(tokenResponse));
            return this;
        }

        public HelseIdSelvbetjeningServiceBuilder WithDefaultConfiguration()
        {
            Options.Value.Returns(new SelvbetjeningConfiguration
            {
                Authority = "https://authority",
                BaseAddress = "https://nhn.selvbetjening"
            });
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

        public HelseIdSelvbetjeningServiceBuilder WithGetClientSecretResponse(IEnumerable<HelseIdSelvbetjening.Infrastructure.Dtos.ClientSecret>? clientSecrets = default!, ProblemDetail? problemDetail = null)
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
                Options,
                TokenService,
                SelvbetjeningApi,
                Logger
            );
        }
    }
}
