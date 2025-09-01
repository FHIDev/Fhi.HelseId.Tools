using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Fhi.HelseIdSelvbetjening.UnitTests.Setup;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Services
{
    public class UpdateClientSecretTests
    {
        [Test]
        public async Task UpdateClientSecret_InvalidClient_ReturnError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDPopTokenResponse(new TokenResponse(null, true, "invalid_token", System.Net.HttpStatusCode.BadRequest));
            var service = builder.Build();

            var response = await service.UpdateClientSecret(new ClientConfiguration("invalid-client", "old-jwk"), "https://authority", "https://nhn.selvbetjening", "new-jwk");

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
    }
}
