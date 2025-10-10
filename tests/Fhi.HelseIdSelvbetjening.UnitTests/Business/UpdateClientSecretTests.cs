using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.UnitTests.Setup;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Business
{
    public class UpdateClientSecretTests
    {
        [Test]
        public async Task UpdateClientSecret_InvalidClient_ReturnError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDPopTokenResponse(new TokenResponse(null, true, "invalid_token"));
            var service = builder.Build();

            var response = await service.UpdateClientSecret(new ClientConfiguration("invalid-client", "old-jwk"), "https://authority", "https://nhn.selvbetjening", "new-jwk");

            using (Assert.EnterMultipleScope())
            {
                var error = GetErrorResult(response);
                Assert.That(error.IsValid, Is.False);

                var errorMessages = error.Errors.Select(e => e.ErrorMessageText);
                Assert.That(errorMessages, Contains.Item("Token request failed: invalid_token"));
            }
        }

        private static ErrorResult GetErrorResult(IResult<ClientSecretUpdateResponse, ErrorResult> response)
        {
            return response.HandleResponse(
                          onSuccess: (clientsecret) => throw new InvalidOperationException("Expected validation error for null ClientConfiguration, but got success response."),
                          onError: (error) => error!);
        }
    }
}
