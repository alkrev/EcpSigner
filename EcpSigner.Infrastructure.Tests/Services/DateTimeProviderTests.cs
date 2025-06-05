using System;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace EcpSigner.Infrastructure.Services
{
    public class DateTimeProviderTests
    {
        [Fact]
        public void Now_ShouldReturnCurrentDateTime()
        {
            // Arrange
            IDateTimeProvider provider = new DateTimeProvider();
            var before = DateTime.Now;

            // Act
            var result = provider.Now;

            var after = DateTime.Now;

            // Assert
            result.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }
    }
}
