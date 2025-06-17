using EcpSigner.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EcpSigner
{
    public class AppEntryPointTests
    {
        [Fact]
        public void Run_ShouldCreateLogger_AndCallBootstrapperRun_AndShutdownLogger()
        {
            // Arrange
            var mockBootstrapper = new Mock<IBootstrapper>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();

            var args = new[] { "arg1", "arg2" };

            mockLoggerFactory
                .Setup(f => f.Create("EcpSigner"))
                .Returns(mockLogger.Object);

            var entryPoint = new AppEntryPoint(mockBootstrapper.Object, mockLoggerFactory.Object);

            // Act
            entryPoint.Run(args);

            // Assert
            mockLoggerFactory.Verify(f => f.Create("EcpSigner"), Times.Once);
            mockBootstrapper.Verify(b => b.Run(args, mockLogger.Object), Times.Once);
            mockLogger.Verify(l => l.Shutdown(), Times.Once);
        }

        [Fact]
        public void Run_ShouldLogFatal_IfBootstrapperThrowsException()
        {
            // Arrange
            var mockBootstrapper = new Mock<IBootstrapper>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            var args = new[] { "arg1" };
            var exception = new InvalidOperationException("Test error");

            mockLoggerFactory
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns(mockLogger.Object);

            mockBootstrapper
                .Setup(b => b.Run(It.IsAny<string[]>(), It.IsAny<ILogger>()))
                .Throws(exception);

            var entryPoint = new AppEntryPoint(mockBootstrapper.Object, mockLoggerFactory.Object);

            // Act
            Action act = () => entryPoint.Run(args);

            // Assert
            act.Should().NotThrow();
            mockLogger.Verify(l => l.Fatal($"AppEntryPoint: {exception.Message}"), Times.Once);
            mockLogger.Verify(l => l.Shutdown(), Times.Once);
        }
    }
}
