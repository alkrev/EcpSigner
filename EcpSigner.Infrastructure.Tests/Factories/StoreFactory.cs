using Xunit;
using FluentAssertions;
using EcpSigner.Infrastructure.Factories;
using CryptographyTools.Store;

namespace EcpSigner.Infrastructure.Factories
{
    public class StoreFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnCurrentUserStoreInstance()
        {
            // Arrange
            var factory = new StoreFactory();

            // Act
            var result = factory.Create();

            // Assert
            result.Should().BeOfType<CurrentUserStore>();
        }
    }
}
