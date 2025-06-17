using System.Net;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Services
{
    public class HelseIdSelvbetjeningServiceUpdateClientSecretTests
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
            }        }
    }
}
