using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Services
{
    public class HelseIdSelvbetjeningServiceTests
    {
        [Test]
        public async Task UpdateClientSecret_InvalidClient_ReturnError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse(null, true, "invalid_token", System.Net.HttpStatusCode.BadRequest));
            var service = builder.Build();

            var response = await service.UpdateClientSecret(new ClientConfiguration("invalid-client", "old-jwk"), "new-jwk");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.Message, Is.EqualTo("invalid_token"));
            }

            builder.Logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Start updating client")),
                null,
                Arg.Any<Func<object, Exception?, string>>()
            );
            builder.Logger.Received(1).Log(
               LogLevel.Error,
               Arg.Any<EventId>(),
               Arg.Is<object>(o => o.ToString()!.Contains("Could not update client invalid-client. StatusCode: BadRequest  Error: invalid_token")),
               null,
               Arg.Any<Func<object, Exception?, string>>()
           );
        }

        [Test]
        public async Task UpdateClientSecret_ValidClient_ReturnOk()
        {
            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"ok\"}")
            });

            var httpClient = new HttpClient(handler);

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("", false, null, HttpStatusCode.OK))
                .WithHttpClient(httpClient);

            var service = builder.Build();

            var response = await service.UpdateClientSecret(new ClientConfiguration("client", "old-jwk"), "new-jwk");
            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("successfully updated client secret"));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_InvalidClient_ReturnError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse(null, true, "invalid_token", HttpStatusCode.BadRequest));
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("invalid-client", "private-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.Message, Is.EqualTo("invalid_token"));
                Assert.That(response.ExpirationDate, Is.Null);
            }

            builder.Logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Reading client secret expiration for client")),
                null,
                Arg.Any<Func<object, Exception?, string>>()
            );
            builder.Logger.Received(1).Log(
               LogLevel.Error,
               Arg.Any<EventId>(),
               Arg.Is<object>(o => o.ToString()!.Contains("Could not read client secret expiration for invalid-client. StatusCode: BadRequest  Error: invalid_token")),
               null,
               Arg.Any<Func<object, Exception?, string>>()
           );
        }

        [Test]
        public async Task ReadClientSecretExpiration_ValidClient_ReturnOkWithExpirationDate()
        {
            var expirationDate = DateTime.UtcNow.AddDays(30);
            var responseJson = $"{{\"expirationDate\":\"{expirationDate:yyyy-MM-ddTHH:mm:ssZ}\"}}";
            
            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            });

            var httpClient = new HttpClient(handler);

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("access-token", false, null, HttpStatusCode.OK))
                .WithHttpClient(httpClient);

            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("client", "private-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("Successfully retrieved client secret expiration"));
                Assert.That(response.ExpirationDate, Is.Not.Null);
                Assert.That(response.ExpirationDate!.Value.Date, Is.EqualTo(expirationDate.Date));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_ValidClient_ReturnOkWithUnixTimestamp()
        {
            var expirationDate = DateTime.UtcNow.AddDays(30);
            var unixTimestamp = ((DateTimeOffset)expirationDate).ToUnixTimeSeconds();
            var responseJson = $"{{\"exp\":{unixTimestamp}}}";
            
            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            });

            var httpClient = new HttpClient(handler);

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("access-token", false, null, HttpStatusCode.OK))
                .WithHttpClient(httpClient);

            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("client", "private-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("Successfully retrieved client secret expiration"));
                Assert.That(response.ExpirationDate, Is.Not.Null);
                Assert.That(response.ExpirationDate!.Value.Date, Is.EqualTo(expirationDate.Date));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_ValidClient_NoExpirationInResponse_ReturnOkWithoutDate()
        {
            var responseJson = "{\"someOtherField\":\"value\"}";
            
            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            });

            var httpClient = new HttpClient(handler);

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("access-token", false, null, HttpStatusCode.OK))
                .WithHttpClient(httpClient);

            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("client", "private-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("Successfully retrieved client secret expiration"));
                Assert.That(response.ExpirationDate, Is.Null);
            }
        }
    }

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
