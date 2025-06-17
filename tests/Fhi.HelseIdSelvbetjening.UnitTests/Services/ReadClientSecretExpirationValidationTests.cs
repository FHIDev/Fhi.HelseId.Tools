using System.Net;
using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;

namespace Fhi.HelseIdSelvbetjening.UnitTests.Services
{
    public class ReadClientSecretExpirationValidationTests
    {
        [Test]
        public async Task ReadClientSecretExpiration_NullClientConfiguration_ShouldReturnValidationError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions();
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(null!);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HasValidationErrors, Is.True);
                Assert.That(response.ValidationErrors, Is.Not.Null);
                Assert.That(response.ValidationErrors, Contains.Item("Client configuration cannot be null"));
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_NullClientId_ShouldReturnValidationError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions();
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration(null!, "valid-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HasValidationErrors, Is.True);
                Assert.That(response.ValidationErrors, Is.Not.Null);
                Assert.That(response.ValidationErrors, Contains.Item("ClientId cannot be null or empty"));
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_EmptyClientId_ShouldReturnValidationError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions();
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("", "valid-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HasValidationErrors, Is.True);
                Assert.That(response.ValidationErrors, Is.Not.Null);
                Assert.That(response.ValidationErrors, Contains.Item("ClientId cannot be null or empty"));
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WhitespaceClientId_ShouldReturnValidationError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions();
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("   ", "valid-jwk"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HasValidationErrors, Is.True);
                Assert.That(response.ValidationErrors, Is.Not.Null);
                Assert.That(response.ValidationErrors, Contains.Item("ClientId cannot be null or empty"));
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_NullJwk_ShouldReturnValidationError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions();
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("valid-client-id", null!));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HasValidationErrors, Is.True);
                Assert.That(response.ValidationErrors, Is.Not.Null);
                Assert.That(response.ValidationErrors, Contains.Item("Jwk cannot be null or empty"));
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_EmptyJwk_ShouldReturnValidationError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions();
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("valid-client-id", ""));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HasValidationErrors, Is.True);
                Assert.That(response.ValidationErrors, Is.Not.Null);
                Assert.That(response.ValidationErrors, Contains.Item("Jwk cannot be null or empty"));
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_WhitespaceJwk_ShouldReturnValidationError()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions();
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("valid-client-id", "   \n\t  "));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HasValidationErrors, Is.True);
                Assert.That(response.ValidationErrors, Is.Not.Null);
                Assert.That(response.ValidationErrors, Contains.Item("Jwk cannot be null or empty"));
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async Task ReadClientSecretExpiration_MultipleValidationErrors_ShouldReturnAllErrors()
        {
            var builder = new HelseIdSelvbetjeningServiceBuilder()
                .WithDefaultOptions();
            var service = builder.Build();

            var response = await service.ReadClientSecretExpiration(new ClientConfiguration("", ""));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.HasValidationErrors, Is.True);
                Assert.That(response.ValidationErrors, Is.Not.Null);
                Assert.That(response.ValidationErrors, Has.Count.EqualTo(2));
                Assert.That(response.ValidationErrors, Contains.Item("ClientId cannot be null or empty"));
                Assert.That(response.ValidationErrors, Contains.Item("Jwk cannot be null or empty"));
                Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.ExpirationDate, Is.EqualTo(DateTime.MinValue));
            }
        }
    }
}
