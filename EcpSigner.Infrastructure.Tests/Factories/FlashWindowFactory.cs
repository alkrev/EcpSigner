using Xunit;
using FluentAssertions;
using EcpSigner.Infrastructure.Factories;
using WindowsTools;

namespace EcpSigner.Infrastructure.Factories
{
    public class FlashWindowFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnFlashWindowInstance()
        {
            // Arrange
            var factory = new FlashWindowFactory();

            // Act
            var result = factory.Create();

            // Assert
            result.Should().BeOfType<FlashWindow>();
        }
    }
}
