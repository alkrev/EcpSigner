using Xunit;
using Moq;
using FluentAssertions;
using EcpSigner.Infrastructure.Services;
using System.Collections.Generic;

namespace EcpSigner.Infrastructure.Services
{
    public class CacheServiceTests
    {
        private readonly Mock<CachingTools.ICache> _cacheMock;
        private readonly CacheService _cacheService;

        public CacheServiceTests()
        {
            _cacheMock = new Mock<CachingTools.ICache>();
            _cacheService = new CacheService(_cacheMock.Object);
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenItemExistsInCache()
        {
            // Arrange
            var number = "123";
            _cacheMock.Setup(c => c.Contains(number)).Returns(true);

            // Act
            var result = _cacheService.Contains(number);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnFalse_WhenItemDoesNotExistInCache()
        {
            // Arrange
            var number = "456";
            _cacheMock.Setup(c => c.Contains(number)).Returns(false);

            // Act
            var result = _cacheService.Contains(number);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Count_ShouldReturnCacheCount()
        {
            // Arrange
            _cacheMock.Setup(c => c.Count()).Returns(5);

            // Act
            var count = _cacheService.Count();

            // Assert
            count.Should().Be(5);
        }

        [Fact]
        public void RemoveExpired_ShouldCallRemoveExpiredOnCache()
        {
            // Act
            _cacheService.RemoveExpired();

            // Assert
            _cacheMock.Verify(c => c.RemoveExpired(), Times.Once);
        }

        [Fact]
        public void SetRange_ShouldPassListToCache()
        {
            // Arrange
            var numbers = new List<string> { "123", "456" };

            // Act
            _cacheService.SetRange(numbers);

            // Assert
            _cacheMock.Verify(c => c.SetRange(numbers), Times.Once);
        }
    }
}
