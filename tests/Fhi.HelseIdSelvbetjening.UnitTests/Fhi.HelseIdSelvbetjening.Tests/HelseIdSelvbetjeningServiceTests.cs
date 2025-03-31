using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Net;
using System.Text.Json;

namespace Fhi.HelseId.ClientSecret.App.Tests.Fhi.HelseIdSelvbetjening.Tests
{
    [TestFixture]
    public class HelseIdSelvbetjeningServiceTests : IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HelseIdSelvbetjeningService> _logger;
        private readonly IOptions<SelvbetjeningConfiguration> _selvbetjeningConfig;
        private readonly HttpClient _httpClient;
        private readonly MockHttpMessageHandler _mockHttpHandler;

        public HelseIdSelvbetjeningServiceTests()
        {
            // Setup mocks
            _logger = Substitute.For<ILogger<HelseIdSelvbetjeningService>>();
            _selvbetjeningConfig = Substitute.For<IOptions<SelvbetjeningConfiguration>>();
            _selvbetjeningConfig.Value.Returns(new SelvbetjeningConfiguration
            {
                Authority = "https://test.authority",
                BaseAddress = "https://test.baseaddress",
                ClientSecretEndpoint = "/api/client/secret"
            });
            
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _mockHttpHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHttpHandler);
            _httpClientFactory.CreateClient().Returns(_httpClient);
        }

        [Test]
        public async Task UpdateClientSecret_SuccessfulUpdate_ReturnsSuccessResponse()
        {
            // Arrange
            var clientId = "test-client-id";
            var clientJwk = CreatePrivateJwk("test-kid");
            var newPublicJwk = CreatePublicJwk("new-kid");
            
            var clientConfig = new ClientConfiguration(clientId, clientJwk);

            // Setup the mock HTTP handler for the discovery document response
            // Note: The issuer must match the authority for the test to pass
            var discoveryJson = JsonSerializer.Serialize(new
            {
                issuer = "https://test.authority",
                token_endpoint = "https://test.authority/connect/token",
                jwks_uri = "https://test.authority/.well-known/openid-configuration/jwks"
            });
            
            // Mock first request (discovery document)
            _mockHttpHandler.AddResponse(
                HttpMethod.Get,
                "https://test.authority/.well-known/openid-configuration",
                HttpStatusCode.OK, 
                discoveryJson
            );
            
            // Mock JWKS endpoint
            _mockHttpHandler.AddResponse(
                HttpMethod.Get,
                "https://test.authority/.well-known/openid-configuration/jwks",
                HttpStatusCode.OK,
                "{ \"keys\": [] }"
            );

            // Mock second request (token request with nonce requirement)
            _mockHttpHandler.AddResponse(
                HttpMethod.Post, 
                "https://test.authority/connect/token",
                HttpStatusCode.BadRequest, 
                "{ \"error\": \"use_dpop_nonce\", \"dpop_nonce\": \"test-nonce\" }"
            );

            // Mock third request (token request with nonce)
            _mockHttpHandler.AddResponse(
                HttpMethod.Post, 
                "https://test.authority/connect/token",
                HttpStatusCode.OK, 
                "{ \"access_token\": \"test-token\", \"token_type\": \"DPoP\", \"expires_in\": 3600 }"
            );

            // Mock fourth request (client secret update)
            _mockHttpHandler.AddResponse(
                HttpMethod.Post, 
                "https://test.baseaddress/api/client/secret",
                HttpStatusCode.OK, 
                "{ \"success\": true }"
            );

            var service = new HelseIdSelvbetjeningService(_selvbetjeningConfig, _httpClientFactory, _logger);

            // Act
            var result = await service.UpdateClientSecret(clientConfig, newPublicJwk);

            // Assert
            Assert.That(result.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Message, Is.EqualTo("successfully updated client secret"));
        }

        [Test]
        public void UpdateClientSecret_TokenRequestFails_ThrowsException()
        {
            // Arrange
            var clientId = "test-client-id";
            var clientJwk = CreatePrivateJwk("test-kid");
            var newPublicJwk = CreatePublicJwk("new-kid");
            
            var clientConfig = new ClientConfiguration(clientId, clientJwk);

            // Setup the mock HTTP handler for the discovery document response
            var discoveryJson = JsonSerializer.Serialize(new
            {
                issuer = "https://test.authority",
                token_endpoint = "https://test.authority/connect/token",
                jwks_uri = "https://test.authority/.well-known/openid-configuration/jwks"
            });
            
            // Mock first request (discovery document)
            _mockHttpHandler.AddResponse(
                HttpMethod.Get,
                "https://test.authority/.well-known/openid-configuration",
                HttpStatusCode.OK, 
                discoveryJson
            );
            
            // Mock JWKS endpoint
            _mockHttpHandler.AddResponse(
                HttpMethod.Get,
                "https://test.authority/.well-known/openid-configuration/jwks",
                HttpStatusCode.OK,
                "{ \"keys\": [] }"
            );

            // Mock second request (token request with error)
            _mockHttpHandler.AddResponse(
                HttpMethod.Post,
                "https://test.authority/connect/token",
                HttpStatusCode.BadRequest, 
                "{ \"error\": \"invalid_client\", \"error_description\": \"Invalid client authentication\" }"
            );

            var service = new HelseIdSelvbetjeningService(_selvbetjeningConfig, _httpClientFactory, _logger);

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(() => service.UpdateClientSecret(clientConfig, newPublicJwk));
            
            // The error message should be the error from the token response
            Assert.That(exception.Message, Is.EqualTo("invalid_client"));
        }

        [Test]
        public void UpdateClientSecret_DiscoveryFails_ThrowsException()
        {
            // Arrange
            var clientId = "test-client-id";
            var clientJwk = CreatePrivateJwk("test-kid");
            var newPublicJwk = CreatePublicJwk("new-kid");
            
            var clientConfig = new ClientConfiguration(clientId, clientJwk);

            // Mock discovery request to fail
            _mockHttpHandler.AddResponse(
                HttpMethod.Get,
                "https://test.authority/.well-known/openid-configuration",
                HttpStatusCode.InternalServerError, 
                "{ \"error\": \"server_error\" }"
            );

            var service = new HelseIdSelvbetjeningService(_selvbetjeningConfig, _httpClientFactory, _logger);

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(() => service.UpdateClientSecret(clientConfig, newPublicJwk));
            
            // The error message starts with this prefix but may include more details
            Assert.That(exception.Message, Does.StartWith("Error connecting to"));
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        #region Helper Methods

        private static string CreatePrivateJwk(string kid)
        {
            return @"{
                ""kid"": """ + kid + @""",
                ""kty"": ""RSA"",
                ""alg"": ""RS256"",
                ""n"": ""0vx7agoebGcQSuuPiLJXZptN9nndrQmbXEps2aiAFbWhM78LhWx4cbbfAAtVT86zwu1RK7aPFFxuhDR1L6tSoc_BJECPebWKRXjBZCiFV4n3oknjhMstn64tZ_2W-5JsGY4Hc5n9yBXArwl93lqt7_RN5w6Cf0h4QyQ5v-65YGjQR0_FDW2QvzqY368QQMicAtaSqzs8KJZgnYb9c7d0zgdAZHzu6qMQvRL5hajrn1n91CbOpbISD08qNLyrdkt-bFTWhAI4vMQFh6WeZu0fM4lFd2NcRwr3XPksINHaQ-G_xBniIqbw0Ls1jF44-csFCur-kEgU8awapJzKnqDKgw"",
                ""e"": ""AQAB"",
                ""d"": ""X4cTteJY_gn4FYPsXB8rdXix5vwsg1FLN5E3EaG6RJoVH-HLLKD9M7dx5oo7GURknchnrRweUkC7hT5fJLM0WbFAKNLWY2vv7B6NqXSzUvxT0_YSfqijwp3RTzlBaCxWp4doFk5N2o8Gy_nHNKroADIkJ46pRUohsXywbReAdYaMwFs9tv8d_cPVY3i07a3t8MN6TNwm0dSawm9v47UiCl3Sk5ZiG7xojPLu4sbg1U2jx4IBTNBznbJSzFHK66jT8bgkuqsk0GjskDJk19Z4qwjwbsnn4j2WBii3RL-Us2lGVkY8fkFzme1z0HbIkfz0Y6mqnOYtqc0X4jfcKoAC8Q"",
                ""p"": ""83i-7IvMGXoMXCskv73TKr8637FiO7Z27zv8oj6pbWUQyLPQBQxtPVnwD20R-60eTDmD2ujnMt5PoqMrm8RfmNhVWDtjjMmCMjOpSXicFHj7XOuVIYQyqVWlWEh6dN36GVZYk93N8Bc9vY41xy8B9RzzOGVQzXvNEvn7O0nVbfs"",
                ""q"": ""3dfOR9cuYq-0S-mkFLzgItgMEfFzB2q3hWehMuG0oCuqnb3vobLyumqjVZQO1dIrdwgTnCdpYzBcOfW5r370AFXjiWft_NGEiovonizhKpo9VVS78TzFgxkIdrecRezsZ-1kYd_s1qDbxtkDEgfAITAG9LUnADun4vIcb6yelxk"",
                ""dp"": ""G4sPXkc6Ya9y8oJW9_ILj4xuppu0lzi_H7VTkS8xj5SdX3coE0oimYwxIi2emTAue0UOa5dpgFGyBJ4c8tQ2VF402XRugKDTP8akYhFo5tAA77Qe_NmtuYZc3C3m3I24G2GvR5sSDxUyAN2zq8Lfn9EUms6rY3Ob8YeiKkTiBj0"",
                ""dq"": ""s9lAH9fggBsoFR8Oac2R_E2gw282rT2kGOAhvIllETE1efrA6huUUvMfBcMpn8lqeW6vzznYY5SSQF7pMdC_agI3nG8Ibp1BUb0JUiraRNqUfLhcQb_d9GF4Dh7e74WbRsobRonujTYN1xCaP6TO61jvWrX-L18txXw494Q_cgk"",
                ""qi"": ""GyM_p6JrXySiz1toFgKbWV-JdI3jQ4ypu9rbMWx3rQJBfmt0FoYzgUIZEVFEcOqwemRN81zoDAaa-Bk0KWNGDjJHZDdDmFhW3AN7lI-puxk_mHZGJ11rxyR8O55XLSe3SPmRfKwZI6yU24ZxvQKFYItdldUKGzO6Ia6zTKhAVRU""
            }";
        }

        private static string CreatePublicJwk(string kid)
        {
            return @"{
                ""kid"": """ + kid + @""",
                ""kty"": ""RSA"",
                ""alg"": ""RS256"",
                ""n"": ""0vx7agoebGcQSuuPiLJXZptN9nndrQmbXEps2aiAFbWhM78LhWx4cbbfAAtVT86zwu1RK7aPFFxuhDR1L6tSoc_BJECPebWKRXjBZCiFV4n3oknjhMstn64tZ_2W-5JsGY4Hc5n9yBXArwl93lqt7_RN5w6Cf0h4QyQ5v-65YGjQR0_FDW2QvzqY368QQMicAtaSqzs8KJZgnYb9c7d0zgdAZHzu6qMQvRL5hajrn1n91CbOpbISD08qNLyrdkt-bFTWhAI4vMQFh6WeZu0fM4lFd2NcRwr3XPksINHaQ-G_xBniIqbw0Ls1jF44-csFCur-kEgU8awapJzKnqDKgw"",
                ""e"": ""AQAB""
            }";
        }

        #endregion
    }
}