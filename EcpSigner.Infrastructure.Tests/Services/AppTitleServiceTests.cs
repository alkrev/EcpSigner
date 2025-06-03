using Xunit;
using FluentAssertions;
using Moq;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using System;
using System.Reflection;

namespace EcpSigner.Infrastructure.Services
{
    public class AppTitleServiceTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly AppTitleService _service;

        public AppTitleServiceTests()
        {
            _loggerMock = new Mock<ILogger>();
            _service = new AppTitleService(_loggerMock.Object);
        }

        [Fact]
        public void GetAppTitle_ShouldReturnTitleWithVersion()
        {
            // Arrange
            var expectedName = Assembly.GetEntryAssembly().GetName().Name;
            var expectedVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

            // Act
            var title = _service.GetAppTitle();

            // Assert
            title.Should().Be($"{expectedName} v{expectedVersion}");
        }

        [Fact]
        public void Set_ShouldLogTitleAndSetConsoleTitle()
        {
            // Arrange
            var expectedTitle = _service.GetAppTitle();
            string originalTitle = Console.Title;

            try
            {
                // Act
                _service.Set();

                // Assert
                _loggerMock.Verify(l => l.Info(expectedTitle), Times.Once);
                Console.Title.Should().Be(expectedTitle);
            }
            finally
            {
                // Cleanup: восстановим оригинальный заголовок консоли
                Console.Title = originalTitle;
            }
        }
    }
}
