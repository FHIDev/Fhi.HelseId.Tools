using Fhi.HelseIdSelvbetjening.Http;
using Fhi.IdentityModel.Tokens;

namespace Fhi.HelseIdSelvbetjening.UnitTests
{
    public class ClientCredentialRequestBuilderTests
    {
        [Test]
        public void GenerateNewRSAJwk_AndUseItToGenerateClientAssertion_RequestContainClientAssertion()
        {
            var jwk = JwkGenerator.GenerateRsaJwk();

            var builder = new ClientCredentialRequestBuilder();
            builder
                .Create("/token-endpoint", "clientId")
                .WithClientAssertion("issuer", jwk.PrivateKey);

            var request = builder.Build();

            Assert.That(request.ClientAssertion.Value, Is.Not.Null);
            Assert.That(request.ClientAssertion.Type, Is.EqualTo("urn:ietf:params:oauth:client-assertion-type:jwt-bearer"));
        }
    }
}
