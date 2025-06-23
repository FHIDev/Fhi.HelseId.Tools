using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using NSubstitute;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests
{
    internal class SelvbetjeningServiceBuilder
    {
        public IHelseIdSelvbetjeningService _selvbetjeningServiceMock { get; private set; }

        public SelvbetjeningServiceBuilder()
        {
            //LoggerMock = Substitute.For<ILogger<ClientSecretExpirationReaderService>>();
            _selvbetjeningServiceMock = Substitute.For<IHelseIdSelvbetjeningService>();
        }

        public SelvbetjeningServiceBuilder WithReadClientSecretExpirationResponse(ClientSecretExpirationResponse clientSecretExpirationResponse)
        {
            _selvbetjeningServiceMock.ReadClientSecretExpiration(Arg.Any<ClientConfiguration>()).Returns(clientSecretExpirationResponse);
            return this;
        }
        public IHelseIdSelvbetjeningService Build() => _selvbetjeningServiceMock;
    }
}
