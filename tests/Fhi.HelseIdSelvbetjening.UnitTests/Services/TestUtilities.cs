using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Services
{
    internal class HelseIdSelvbetjeningServiceBuilder
    {
        public ILogger<HelseIdSelvbetjeningService> Logger { get; private set; } = Substitute.For<ILogger<HelseIdSelvbetjeningService>>();
        public IHttpClientFactory HttpClientFactory { get; private set; } = Substitute.For<IHttpClientFactory>();
        public Microsoft.Extensions.Options.IOptions<SelvbetjeningConfiguration> Options { get; private set; }
            = Substitute.For<Microsoft.Extensions.Options.IOptions<SelvbetjeningConfiguration>>();
        public ITokenService TokenService { get; private set; } = Substitute.For<ITokenService>();
        private HttpClient? _httpClient;

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

        public HelseIdSelvbetjeningServiceBuilder WithDefaultOptions()
        {
            Options.Value.Returns(new SelvbetjeningConfiguration
            {
                Authority = "https://authority",
                BaseAddress = "https://nhn.selvbetjening",
                ClientSecretEndpoint = "client-secret"
            });
            return this;
        }

        public HelseIdSelvbetjeningServiceBuilder WithHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            HttpClientFactory.CreateClient(Arg.Any<string>()).Returns(_httpClient);
            HttpClientFactory.CreateClient().Returns(_httpClient);
            return this;
        }

        public HelseIdSelvbetjeningService Build()
        {
            return new HelseIdSelvbetjeningService(
                Options,
                HttpClientFactory,
                TokenService,
                Logger
            );
        }
    }

    internal class TestHttpMessageHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage _response;

        public TestHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
