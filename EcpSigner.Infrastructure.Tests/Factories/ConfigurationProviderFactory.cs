using Xunit;
using Moq;
using FluentAssertions;
using EcpSigner.Infrastructure.Factories;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Configuration;

namespace EcpSigner.Infrastructure.Factories
{
    public class ConfigurationProviderFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnJsonConfigurationProvider_WithCorrectParams()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var configPath = "testConfig.json";
            var factory = new ConfigurationProviderFactory(loggerMock.Object, configPath);

            // Act
            var result = factory.Create();

            // Assert
            result.Should().BeOfType<JsonConfigurationProvider>();
        }
    }
}
