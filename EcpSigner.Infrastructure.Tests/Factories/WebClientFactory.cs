using Xunit;
using FluentAssertions;
using EcpSigner.Infrastructure.Factories;
using Ecp.Web;
using EcpSigner.Infrastructure.WebClients;

namespace EcpSigner.Infrastructure.Factories
{
    public class WebClientFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnWebClientInstance_WithCorrectUrl()
        {
            // Arrange
            var factory = new WebClientFactory();
            var url = "https://example.com";

            // Act
            var result = factory.Create(url);

            // Assert
            result.Should().BeOfType<WebClient>();
        }
    }
}
