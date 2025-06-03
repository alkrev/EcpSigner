using Xunit;
using FluentAssertions;
using EcpSigner.Infrastructure.Factories;
using CryptographyTools.Signing.CryptoPro;

namespace EcpSigner.Infrastructure.Factories
{
    public class CryptoFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnCryptoInstance()
        {
            // Arrange
            var factory = new CryptoFactory();

            // Act
            var result = factory.Create();

            // Assert
            result.Should().BeOfType<Crypto>();
        }
    }
}
