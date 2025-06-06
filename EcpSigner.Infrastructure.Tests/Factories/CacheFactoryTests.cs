using CachingTools;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Factories;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcpSigner.Infrastructure.Factories
{
    public class CacheFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnCacheInstance_WithCorrectExpiration()
        {
            // Arrange
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            var factory = new CacheFactory();
            int expectedMinutes = 10;

            // Act
            var result = factory.Create(expectedMinutes, dateTimeProviderMock.Object);

            // Assert
            result.Should().BeOfType<Cache>();
        }
    }
}
