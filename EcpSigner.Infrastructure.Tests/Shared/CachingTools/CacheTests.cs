using System;
using System.Collections.Generic;
using CachingTools;
using EcpSigner.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcpSigner.Infrastructure.Shared.CachingTools
{
    public class CacheTests
    {
        private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
        private DateTime _now;

        public CacheTests()
        {
            _mockDateTimeProvider = new Mock<IDateTimeProvider>();
            _now = new DateTime(2025, 1, 1, 12, 0, 0);
            _mockDateTimeProvider.Setup(dp => dp.Now).Returns(() => _now);
        }

        [Fact]
        public void SetRange_Should_Add_Entries_With_Expiration()
        {
            // Arrange
            var cache = new Cache(5, _mockDateTimeProvider.Object);
            var keys = new List<string> { "123", "456" };

            // Act
            cache.SetRange(keys);

            // Assert
            cache.Count().Should().Be(2);
            cache.Contains("123").Should().BeTrue();
            cache.Contains("456").Should().BeTrue();
        }

        [Fact]
        public void Contains_Should_Return_False_If_Item_Is_Expired()
        {
            // Arrange
            var cache = new Cache(1, _mockDateTimeProvider.Object);
            var keys = new List<string> { "123" };
            cache.SetRange(keys);

            // "Перемотка" времени вперёд
            _now = _now.AddMinutes(2);

            // Act
            bool result = cache.Contains("123");

            // Assert
            result.Should().BeFalse();
            cache.Count().Should().Be(0); // удалено
        }

        [Fact]
        public void Contains_Should_Return_True_If_Not_Expired()
        {
            var cache = new Cache(10, _mockDateTimeProvider.Object);
            cache.SetRange(new List<string> { "abc" });

            _now = _now.AddMinutes(5);

            cache.Contains("abc").Should().BeTrue();
        }

        [Fact]
        public void RemoveExpired_Should_Remove_Only_Expired_Entries()
        {
            var cache = new Cache(1, _mockDateTimeProvider.Object);
            cache.SetRange(new List<string> { "valid", "expired" });

            // Устанавливаем "valid" на 1 минуту, затем перематываем на 2 минуты
            _now = _now.AddMinutes(2);

            cache.RemoveExpired();

            cache.Contains("valid").Should().BeFalse();
            cache.Contains("expired").Should().BeFalse();
            cache.Count().Should().Be(0);
        }

        [Fact]
        public void Count_Should_Return_Correct_Valid_Entries()
        {
            var cache = new Cache(5, _mockDateTimeProvider.Object);
            cache.SetRange(new List<string> { "one", "two" });

            cache.Count().Should().Be(2);
        }
    }
}
