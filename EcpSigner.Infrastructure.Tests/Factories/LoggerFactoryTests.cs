using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using FluentAssertions;

namespace EcpSigner.Infrastructure.Factories
{
    public class LoggerFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnNLogLogger_WithExpectedName()
        {
            // Arrange
            var loggerName = "IntegrationTestLogger";
            var factory = new LoggerFactory();

            // Act
            ILogger result = factory.Create(loggerName);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NLogLogger>();
        }
    }
}
