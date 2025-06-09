using CachingTools;
using CryptographyTools.Signing.CryptoPro;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public class DateTimeProviderFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnDateTimeProviderFactoryInstance()
        {
            // Arrange
            var factory = new DateTimeProviderFactory();

            // Act
            var result = factory.Create();

            // Assert
            result.Should().BeOfType<DateTimeProvider>();
        }
    }
}
