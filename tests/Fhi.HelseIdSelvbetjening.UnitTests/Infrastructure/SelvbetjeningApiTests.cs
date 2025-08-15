using Fhi.Authentication.Tokens;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening;
using Fhi.HelseIdSelvbetjening.UnitTests.Setup;
using NSubstitute;
using System.Net;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Infrastructure
{
    public class SelvbetjeningApiTests
    {
        [Test]
        public async Task ReadClientSecret_MultipleKeysAndValidResponse_Ok()
        {
            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"[
                {""expiration"":null,""kid"":""-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"",""jwkThumbprint"":""-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"",""origin"":""Gui"",""publicJwk"":null},
                {""expiration"":null,""kid"":""ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk"",""jwkThumbprint"":""ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk"",""origin"":""Gui"",""publicJwk"":null},
                {""expiration"":""2025-06-20T00:00:00Z"",""kid"":""VOLmwuVJtP2NEAW0-Hl2ZRymWcgvyZtPnDivec2dZrM"",""jwkThumbprint"":""VOLmwuVJtP2NEAW0-Hl2ZRymWcgvyZtPnDivec2dZrM"",""origin"":""Api"",""publicJwk"":null}
            ]")
            });

            var httpClient = new HttpClient(handler);
            var factory = Substitute.For<IHttpClientFactory>();
            factory.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var selvbetjeningApi = new SelvbetjeningApi(factory);
            var (ClientSecrets, _) = await selvbetjeningApi.GetClientSecretsAsync(
                "https://nhn.selvbetjening",
                JwkGenerator.GenerateRsaJwk().PrivateKey,
                "accessToken");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ClientSecrets, Is.Not.Null);
                Assert.That(ClientSecrets!.Count(), Is.EqualTo(3));
                Assert.That(ClientSecrets!.FirstOrDefault()!.Kid, Is.EqualTo("-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"));
            }
        }
    }
}
