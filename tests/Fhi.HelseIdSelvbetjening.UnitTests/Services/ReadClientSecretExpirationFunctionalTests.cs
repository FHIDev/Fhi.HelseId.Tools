using System.Net;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Services
{
    public class ReadClientSecretExpirationFunctionalTests
    {
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
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
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
                Assert.That(response.ExpirationDate, Is.Not.EqualTo(DateTime.MinValue));
                Assert.That(response.ExpirationDate.Date, Is.EqualTo(expirationDate.Date));
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
                Assert.That(response.ExpirationDate, Is.Not.EqualTo(DateTime.MinValue));
                Assert.That(response.ExpirationDate.Date, Is.EqualTo(expirationDate.Date));
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
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_ArrayResponseWithExpiration_ReturnExpirationDate()
        {
            var arrayResponse = @"[
                {""expiration"":null,""kid"":""-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"",""jwkThumbprint"":""-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"",""origin"":""Gui"",""publicJwk"":null},
                {""expiration"":null,""kid"":""ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk"",""jwkThumbprint"":""ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk"",""origin"":""Gui"",""publicJwk"":null},
                {""expiration"":""2025-06-20T00:00:00Z"",""kid"":""VOLmwuVJtP2NEAW0-Hl2ZRymWcgvyZtPnDivec2dZrM"",""jwkThumbprint"":""VOLmwuVJtP2NEAW0-Hl2ZRymWcgvyZtPnDivec2dZrM"",""origin"":""Api"",""publicJwk"":null}
            ]";

            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(arrayResponse)
            });

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null, HttpStatusCode.OK))
                .WithHttpClient(new HttpClient(handler));
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("test-client", "test-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("Successfully retrieved client secret expiration"));
                Assert.That(response.ExpirationDate, Is.Not.EqualTo(DateTime.MinValue));
                Assert.That(response.ExpirationDate, Is.EqualTo(new DateTime(2025, 6, 20, 0, 0, 0, DateTimeKind.Utc)));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_ArrayResponseAllNullExpirations_ReturnNoExpiration()
        {
            var arrayResponse = @"[
                {""expiration"":null,""kid"":""-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"",""jwkThumbprint"":""-JYdQcqGy0Qmbpv6pX_2EdJkGciRu7BaDJk3Hz4WdZ4"",""origin"":""Gui"",""publicJwk"":null},
                {""expiration"":null,""kid"":""ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk"",""jwkThumbprint"":""ISinWp6jrRwTF_yhD1FBJ2amBT-uPwumswyRhmdjbWk"",""origin"":""Gui"",""publicJwk"":null}
            ]";

            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(arrayResponse)
            });

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null, HttpStatusCode.OK))
                .WithHttpClient(new HttpClient(handler));
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("test-client", "test-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("Successfully retrieved client secret expiration"));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysFirstExpired_ShouldReturnLatestNotFirst()
        {
            var expiredDate = DateTime.UtcNow.AddDays(-30); // Expired 30 days ago
            var validDate = DateTime.UtcNow.AddDays(60);    // Valid for 60 more days
            
            var arrayResponseWithExpiredFirst = $@"[
                {{""expiration"":""{expiredDate:yyyy-MM-ddTHH:mm:ssZ}"",""kid"":""expired-key-id"",""jwkThumbprint"":""expired-thumb"",""origin"":""Api"",""publicJwk"":null}},
                {{""expiration"":""{validDate:yyyy-MM-ddTHH:mm:ssZ}"",""kid"":""valid-key-id"",""jwkThumbprint"":""valid-thumb"",""origin"":""Api"",""publicJwk"":null}}
            ]";

            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(arrayResponseWithExpiredFirst)
            });

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null, HttpStatusCode.OK))
                .WithHttpClient(new HttpClient(handler));
            var service = builder.Build();

            // Client JWK that should match the second (valid) key
            var clientJwkWithKid = """
            {
                "kid": "valid-key-id",
                "kty": "RSA",
                "d": "test-private-key-data",
                "n": "test-modulus",
                "e": "AQAB"
            }
            """;

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("test-client", clientJwkWithKid));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("Successfully retrieved client secret expiration"));
                Assert.That(response.ExpirationDate, Is.Not.EqualTo(DateTime.MinValue));
                // Truncate to seconds to handle precision differences in JSON parsing
                var expectedValidDate = new DateTime(validDate.Year, validDate.Month, validDate.Day, 
                    validDate.Hour, validDate.Minute, validDate.Second, DateTimeKind.Utc);
                var actualDate = new DateTime(response.ExpirationDate.Year, response.ExpirationDate.Month, 
                    response.ExpirationDate.Day, response.ExpirationDate.Hour, response.ExpirationDate.Minute, 
                    response.ExpirationDate.Second, DateTimeKind.Utc);
                Assert.That(actualDate, Is.EqualTo(expectedValidDate), 
                    "Service should return latest expiration when no kid matches");
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysWithKidMatching_ShouldReturnMatchingKeyExpiration()
        {
            var firstKeyDate = DateTime.UtcNow.AddDays(30);
            var matchingKeyDate = DateTime.UtcNow.AddDays(90);
            var thirdKeyDate = DateTime.UtcNow.AddDays(45);
            
            var arrayResponseWithMultipleKeys = $@"[
                {{""expiration"":""{firstKeyDate:yyyy-MM-ddTHH:mm:ssZ}"",""kid"":""first-key-id"",""jwkThumbprint"":""first-thumb"",""origin"":""Api"",""publicJwk"":null}},
                {{""expiration"":""{matchingKeyDate:yyyy-MM-ddTHH:mm:ssZ}"",""kid"":""target-key-id"",""jwkThumbprint"":""target-thumb"",""origin"":""Api"",""publicJwk"":null}},
                {{""expiration"":""{thirdKeyDate:yyyy-MM-ddTHH:mm:ssZ}"",""kid"":""third-key-id"",""jwkThumbprint"":""third-thumb"",""origin"":""Api"",""publicJwk"":null}}
            ]";

            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(arrayResponseWithMultipleKeys)
            });

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null, HttpStatusCode.OK))
                .WithHttpClient(new HttpClient(handler));
            var service = builder.Build();

            // Client JWK with kid that matches the second key in the response
            var clientJwkWithTargetKid = """
            {
                "kid": "target-key-id",
                "kty": "RSA", 
                "d": "test-private-key-data",
                "n": "test-modulus",
                "e": "AQAB"
            }
            """;

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("test-client", clientJwkWithTargetKid));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("Successfully retrieved client secret expiration"));
                Assert.That(response.ExpirationDate, Is.Not.EqualTo(DateTime.MinValue));
                // Truncate to seconds to handle precision differences in JSON parsing
                var expectedMatchingDate = new DateTime(matchingKeyDate.Year, matchingKeyDate.Month, matchingKeyDate.Day, 
                    matchingKeyDate.Hour, matchingKeyDate.Minute, matchingKeyDate.Second, DateTimeKind.Utc);
                var actualDate = new DateTime(response.ExpirationDate.Year, response.ExpirationDate.Month, 
                    response.ExpirationDate.Day, response.ExpirationDate.Hour, response.ExpirationDate.Minute, 
                    response.ExpirationDate.Second, DateTimeKind.Utc);
                Assert.That(actualDate, Is.EqualTo(expectedMatchingDate),
                    "Service should return expiration for matching kid");
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MultipleKeysNoKidInClientJwk_ShouldReturnLatestExpiration()
        {            
            var firstKeyDate = DateTime.UtcNow.AddDays(15);
            var latestKeyDate = DateTime.UtcNow.AddDays(75);
            var middleKeyDate = DateTime.UtcNow.AddDays(45);
            
            var arrayResponseWithMultipleKeys = $@"[
                {{""expiration"":""{firstKeyDate:yyyy-MM-ddTHH:mm:ssZ}"",""kid"":""first-key-id"",""jwkThumbprint"":""first-thumb"",""origin"":""Api"",""publicJwk"":null}},
                {{""expiration"":""{latestKeyDate:yyyy-MM-ddTHH:mm:ssZ}"",""kid"":""latest-key-id"",""jwkThumbprint"":""latest-thumb"",""origin"":""Api"",""publicJwk"":null}},
                {{""expiration"":""{middleKeyDate:yyyy-MM-ddTHH:mm:ssZ}"",""kid"":""middle-key-id"",""jwkThumbprint"":""middle-thumb"",""origin"":""Api"",""publicJwk"":null}}
            ]";

            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(arrayResponseWithMultipleKeys)
            });

            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions()
                .WithDPopTokenResponse(new TokenResponse("valid-token", false, null, HttpStatusCode.OK))
                .WithHttpClient(new HttpClient(handler));
            var service = builder.Build();

            // Client JWK without kid
            var clientJwkWithoutKid = """
            {
                "kty": "RSA", 
                "d": "test-private-key-data",
                "n": "test-modulus",
                "e": "AQAB"
            }
            """;

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("test-client", clientJwkWithoutKid));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Message, Is.EqualTo("Successfully retrieved client secret expiration"));
                Assert.That(response.ExpirationDate, Is.Not.EqualTo(DateTime.MinValue));
                
                // Should return the latest expiration date since no kid matching is possible
                var expectedLatestDate = new DateTime(latestKeyDate.Year, latestKeyDate.Month, latestKeyDate.Day, 
                    latestKeyDate.Hour, latestKeyDate.Minute, latestKeyDate.Second, DateTimeKind.Utc);
                var actualDate = new DateTime(response.ExpirationDate.Year, response.ExpirationDate.Month, 
                    response.ExpirationDate.Day, response.ExpirationDate.Hour, response.ExpirationDate.Minute, 
                    response.ExpirationDate.Second, DateTimeKind.Utc);
                Assert.That(actualDate, Is.EqualTo(expectedLatestDate),
                    "Service should return latest expiration when client JWK has no kid");
            }
        }
    }
}
