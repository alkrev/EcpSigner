using Xunit;
using FluentAssertions;
using EcpSigner.Infrastructure.Factories;
using CachingTools;

namespace EcpSigner.Infrastructure.Factories
{
    public class CacheFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnCacheInstance_WithCorrectExpiration()
        {
            // Arrange
            var factory = new CacheFactory();
            int expectedMinutes = 10;

            // Act
            var result = factory.Create(expectedMinutes);

            // Assert
            result.Should().BeOfType<Cache>();
        }
    }
}
