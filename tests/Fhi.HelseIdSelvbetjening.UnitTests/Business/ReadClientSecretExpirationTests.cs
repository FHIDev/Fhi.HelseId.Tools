using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.Infrastructure;
using Fhi.HelseIdSelvbetjening.UnitTests.Setup;
using ClientSecretDto = Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening.Dtos.ClientSecret;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Services
{
    public class ReadClientSecretExpirationTests
    {
        /// <summary>
        /// TODO: If provided kid does not match should it return the one with date? Is this really a case from Selvbetjening? The client does not have the key returned so we should not guess.
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore("See todo")]
        public async Task ReadClientSecretExpiration_MultipleKeysWithExpiration_ReturnExpirationDate()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null))
                .WithGetClientSecretResponse(
                [
                    new() { Expiration = null, Kid = "-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4", JwkThumbprint = "-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4", Origin = "Gui", PublicJwk = null},
                    new() { Expiration = null, Kid = "ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk", JwkThumbprint = "-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4", Origin = "Gui", PublicJwk = null},
                    new() { Expiration = DateTime.Parse("2025-06-20T00:00:00Z"), Kid = "VOLmwuVJtP2NEAW0-Hl2ZRymWcgvyZtPnDivec2dZrM", JwkThumbprint = "VOLmwuVJtP2NEAW0-Hl2ZRymWcgvyZtPnDivec2dZrM", Origin = "Api", PublicJwk = null}
                ]);

            var service = builder.Build();
            var response = await service.ReadClientSecretExpiration(new ClientConfiguration(
                "test-client",
                """
                {
                    "kid": "ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk",
                    "kty": "RSA",
                    "d": "test-d-value",
                    "n": "test-n-value",
                    "e": "AQAB"
                }
                
                """), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var successResult = GetSuccessResult(response);
                Assert.That(successResult.SelectedSecret!.ExpirationDate, Is.EqualTo(null));
                Assert.That(successResult.AllSecrets, Has.Count.EqualTo(3));
            }
        }

        /// <summary>
        /// TODO: should we set ExpirationDate to DateTime.MinValue if no expiration is found? Should rather be handled by the CLI
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysWithEmptyExpiration_ShouldReturnSelectedWithEmptyExpiration()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDPopTokenResponse(new TokenResponse("valid -token", false, null))
                .WithGetClientSecretResponse(
                [
                    new() { Expiration = null, Kid = "-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4", JwkThumbprint = "-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4", Origin = "Gui", PublicJwk = null},
                    new() { Expiration = null, Kid = "ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk", JwkThumbprint = "-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4", Origin = "Gui", PublicJwk = null},
                ]);

            var service = builder.Build();
            var response = await service.ReadClientSecretExpiration(new ClientConfiguration(
                "test-client",
                """
                {
                    "kid": "ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk",
                    "kty": "RSA",
                    "d": "test-d-value",
                    "n": "test-n-value",
                    "e": "AQAB"
                }
                
                """), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var successResult = GetSuccessResult(response);
                Assert.That(successResult.SelectedSecret!.ExpirationDate, Is.EqualTo(null));
                Assert.That(successResult.AllSecrets, Has.Count.EqualTo(2));
            }
        }

        /// <summary>
        /// TODO: Remove test? Should not return the valid key as it may not be nown for the client.
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore("See todo")]
        public async Task ReadClientSecretExpiration_MultipleKeysFirstExpired_ReturnLatestNotFirst()
        {
            var expiredDate = DateTime.UtcNow.AddDays(-30);
            var validDate = DateTime.UtcNow.AddDays(60);
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null))
                .WithGetClientSecretResponse(
                [
                    new ClientSecretDto() { Expiration = expiredDate, Kid = "expired-key-id", JwkThumbprint = "-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4", Origin = "Api", PublicJwk = null},
                    new ClientSecretDto() { Expiration = validDate, Kid = "valid-key-id", JwkThumbprint = "ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk", Origin = "Gui", PublicJwk = null},
                ]);

            var service = builder.Build();
            var clientJwkWithKid = """
            {
                "kid": "expired-key-id",
                "kty": "RSA",
                "d": "test-private-key-data",
                "n": "test-modulus",
                "e": "AQAB"
            }
            """;
            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("test-client", clientJwkWithKid), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var successResult = GetSuccessResult(response);
                Assert.That(successResult.SelectedSecret!.ExpirationDate, Is.Not.EqualTo(DateTime.MinValue));
                var expectedValidDate = new DateTime(validDate.Year, validDate.Month, validDate.Day,
                    validDate.Hour, validDate.Minute, validDate.Second, DateTimeKind.Utc);
                var actualDate = new DateTime(successResult.SelectedSecret.ExpirationDate!.Value.Year, successResult.SelectedSecret.ExpirationDate.Value.Month,
                    successResult.SelectedSecret.ExpirationDate!.Value.Day, successResult.SelectedSecret.ExpirationDate!.Value.Hour, successResult.SelectedSecret.ExpirationDate!.Value.Minute,
                    successResult.SelectedSecret.ExpirationDate!.Value.Second, DateTimeKind.Utc);
                Assert.That(actualDate, Is.EqualTo(expectedValidDate),
                    "Service should return latest expiration when no kid matches");
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysWithKidMatching_ReturnMatchingKeyExpiration()
        {
            var firstKeyDate = DateTime.UtcNow.AddDays(30);
            var matchingKeyDate = DateTime.UtcNow.AddDays(90);
            var thirdKeyDate = DateTime.UtcNow.AddDays(45);

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null))
                .WithGetClientSecretResponse(
                [
                    new() { Expiration = firstKeyDate, Kid = "first-key-id", Origin = "Api", PublicJwk = null},
                    new() { Expiration = matchingKeyDate, Kid = "target-key-id",Origin = "Api", PublicJwk = null},
                    new() { Expiration = thirdKeyDate, Kid = "third-key-id",Origin = "Gui", PublicJwk = null},
                ]);

            var service = builder.Build();
            var clientJwkWithTargetKid = """
            {
                "kid": "target-key-id",
                "kty": "RSA", 
                "d": "test-private-key-data",
                "n": "test-modulus",
                "e": "AQAB"
            }
            """;

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("test-client", clientJwkWithTargetKid), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var successResult = GetSuccessResult(response);
                Assert.That(successResult.SelectedSecret!.ExpirationDate, Is.Not.EqualTo(DateTime.MinValue));
                var expectedMatchingDate = new DateTime(matchingKeyDate.Year, matchingKeyDate.Month, matchingKeyDate.Day,
                    matchingKeyDate.Hour, matchingKeyDate.Minute, matchingKeyDate.Second, DateTimeKind.Utc);
                var actualDate = new DateTime(successResult.SelectedSecret.ExpirationDate!.Value.Year, successResult.SelectedSecret.ExpirationDate.Value.Month,
                    successResult.SelectedSecret.ExpirationDate!.Value.Day, successResult.SelectedSecret.ExpirationDate!.Value.Hour, successResult.SelectedSecret.ExpirationDate.Value.Minute,
                    successResult.SelectedSecret.ExpirationDate!.Value.Second, DateTimeKind.Utc);
                Assert.That(actualDate, Is.EqualTo(expectedMatchingDate), "Service should return expiration for matching kid");
            }
        }

        /// <summary>
        /// TODO: Should return multiple responses and let the client (CLI) handle it. Should we return error or message?
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysNoKidInClientJwk_ReturnEmptySelected()
        {
            var firstKeyDate = DateTime.UtcNow.AddDays(15);
            var latestKeyDate = DateTime.UtcNow.AddDays(75);
            var middleKeyDate = DateTime.UtcNow.AddDays(45);
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null))
                .WithGetClientSecretResponse(
                [
                    new() { Expiration = firstKeyDate, Kid = "first-key-id", Origin = "Api", PublicJwk = null},
                    new() { Expiration = latestKeyDate, Kid = "latest-key-id",Origin = "Api", PublicJwk = null},
                    new() { Expiration = middleKeyDate, Kid = "middle-key-id",Origin = "Api", PublicJwk = null},
                ]);
            var service = builder.Build();

            var clientJwkWithoutKid = """
            {
                "kty": "RSA", 
                "d": "test-private-key-data",
                "n": "test-modulus",
                "e": "AQAB"
            }
            """;
            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("test-client", clientJwkWithoutKid), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var successResult = GetSuccessResult(response);
                Assert.That(successResult.SelectedSecret, Is.EqualTo(null));
                Assert.That(successResult.AllSecrets, Has.Count.EqualTo(3));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_InvalidClient_ReturnError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDPopTokenResponse(new TokenResponse(null, true, "invalid_token"));
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("invalid-client", "private-jwk"), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var error = GetErrorResult(response);
                var errorMessages = error.Errors.Select(e => e.ErrorMessageText);
                Assert.That(errorMessages, Contains.Item("Token request failed invalid_token"));
            }
        }

        [TestCase(null!)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task ReadClientSecretExpiration_NullClientId_ReturnValidationError(string clientId)
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder();

            var service = builder.Build();
            var response = await service.ReadClientSecretExpiration(new ClientConfiguration(clientId, "valid-jwk"), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var error = GetErrorResult(response);
                var errorMessages = error.Errors.Select(e => e.ErrorMessageText);
                Assert.That(errorMessages, Contains.Item("ClientId cannot be null or empty"));
            }
        }

        [TestCase(null!)]
        [TestCase("")]
        [TestCase("   \n\t  ")]
        public async Task ReadClientSecretExpiration_NullOrEmptyJwk_ReturnValidationError(string jwk)
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder();

            var service = builder.Build();
            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("valid-client-id", jwk), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var error = GetErrorResult(response);
                var errorMessages = error.Errors.Select(e => e.ErrorMessageText);
                Assert.That(errorMessages, Contains.Item("Jwk cannot be null or empty"));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MultipleValidationErrors_ShouldReturnAllErrors()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder();

            var service = builder.Build();
            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("", ""), "https://authority", "https://nhn.selvbetjening");

            using (Assert.EnterMultipleScope())
            {
                var error = GetErrorResult(response);
                var errorMessages = error.Errors.Select(e => e.ErrorMessageText);
                Assert.That(errorMessages, Contains.Item("ClientId cannot be null or empty"));
                Assert.That(errorMessages, Contains.Item("Jwk cannot be null or empty"));
            }
        }

        private static ClientSecretExpirationResponse GetSuccessResult(IResult<ClientSecretExpirationResponse, ErrorResult> response)
        {
            return response.HandleResponse(
                              onSuccess: (clientsecret) => clientsecret,
                              onError: (error) => throw new InvalidOperationException("Expected success"));
        }
        private static ErrorResult GetErrorResult(IResult<ClientSecretExpirationResponse, ErrorResult> response)
        {
            return response.HandleResponse(
                          onSuccess: (clientsecret) => throw new InvalidOperationException("Expected validation error for null ClientConfiguration, but got success response."),
                          onError: (error) => error!);
        }
    }
}
