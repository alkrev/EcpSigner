using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using FluentAssertions;
using Moq;
using WindowsTools;
using Xunit;

namespace EcpSigner.Infrastructure.Services
{
    public class FlashWindowServiceTests
    {
        private readonly Mock<IFlashWindow> _mockFlashWindow;
        private readonly FlashWindowService _service;

        public FlashWindowServiceTests()
        {
            _mockFlashWindow = new Mock<IFlashWindow>();
            _service = new FlashWindowService(_mockFlashWindow.Object);
        }

        [Fact]
        public void Start_ShouldCallFlashWindowStartAndSetIsFlashingToTrue()
        {
            // Act
            _service.Start();

            // Assert
            _mockFlashWindow.Verify(fw => fw.Start(), Times.Once);
            _service.IsFlashing().Should().BeTrue();
        }

        [Fact]
        public void Stop_ShouldCallFlashWindowStopAndSetIsFlashingToFalse()
        {
            // Arrange
            _service.Start();

            // Act
            _service.Stop();

            // Assert
            _mockFlashWindow.Verify(fw => fw.Stop(), Times.Once);
            _service.IsFlashing().Should().BeFalse();
        }

        [Fact]
        public void IsFlashing_ShouldReturnFalseByDefault()
        {
            // Act
            var result = _service.IsFlashing();

            // Assert
            result.Should().BeFalse();
        }
    }
}
