using Xunit;
using Moq;
using FluentAssertions;
using EcpSigner.Infrastructure.Services;

namespace EcpSigner.Infrastructure.Services
{
    public class NLogLoggerTests
    {
        private readonly Mock<NLog.ILogger> _mockLogger;
        private readonly NLogLogger _nlogLogger;

        public NLogLoggerTests()
        {
            _mockLogger = new Mock<NLog.ILogger>();
            _nlogLogger = new NLogLogger(_mockLogger.Object);
        }

        [Fact]
        public void Debug_ShouldCallLoggerDebug()
        {
            // Arrange
            var message = "Debug message";

            // Act
            _nlogLogger.Debug(message);

            // Assert
            _mockLogger.Verify(l => l.Debug(message), Times.Once);
        }

        [Fact]
        public void Info_ShouldCallLoggerInfo()
        {
            // Arrange
            var message = "Info message";

            // Act
            _nlogLogger.Info(message);

            // Assert
            _mockLogger.Verify(l => l.Info(message), Times.Once);
        }

        [Fact]
        public void Error_ShouldCallLoggerError()
        {
            // Arrange
            var message = "Error message";

            // Act
            _nlogLogger.Error(message);

            // Assert
            _mockLogger.Verify(l => l.Error(message), Times.Once);
        }

        [Fact]
        public void Warn_ShouldCallLoggerWarn()
        {
            // Arrange
            var message = "Warning message";

            // Act
            _nlogLogger.Warn(message);

            // Assert
            _mockLogger.Verify(l => l.Warn(message), Times.Once);
        }

        [Fact]
        public void Fatal_ShouldCallLoggerFatal()
        {
            // Arrange
            var message = "Fatal message";

            // Act
            _nlogLogger.Fatal(message);

            // Assert
            _mockLogger.Verify(l => l.Fatal(message), Times.Once);
        }

        [Fact]
        public void Flush_ShouldNotThrow()
        {
            // Act
            var act = () => _nlogLogger.Flush();

            // Assert
            act.Should().NotThrow(); // Проверяем, что метод не выбрасывает исключение
        }

        [Fact]
        public void Shutdown_ShouldNotThrow()
        {
            // Act
            var act = () => _nlogLogger.Shutdown();

            // Assert
            act.Should().NotThrow(); // Проверяем, что метод не выбрасывает исключение
        }
    }
}
