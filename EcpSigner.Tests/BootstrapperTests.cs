using Xunit;
using FluentAssertions;
using Moq;
using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using EcpSigner.Domain.Interfaces;

namespace EcpSigner
{
    public class BootstrapperTests
    {
        [Fact]
        public void Run_ShouldCallRunnerRunAsync_WhenNoExceptionThrown()
        {
            // Arrange
            var args = new[] { "arg1", "arg2" };
            var loggerMock = new Mock<ILogger>();
            var runnerMock = new Mock<IProgramRunner>();
            runnerMock.Setup(r => r.RunAsync(args)).Returns(Task.CompletedTask);

            var factoryMock = new Mock<IProgramRunnerFactory>();
            factoryMock
                .Setup(f => f.Create("config.json", loggerMock.Object))
                .Returns(runnerMock.Object);

            var bootstrapper = new Bootstrapper(factoryMock.Object);

            // Act
            bootstrapper.Run(args, loggerMock.Object);

            // Assert
            factoryMock.Verify(f => f.Create("config.json", loggerMock.Object), Times.Once);
            runnerMock.Verify(r => r.RunAsync(args), Times.Once);
            loggerMock.Verify(l => l.Fatal(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Run_ShouldLogFatal_WhenExceptionIsThrown()
        {
            // Arrange
            var args = new[] { "arg1", "arg2" };
            var loggerMock = new Mock<ILogger>();
            var exception = new InvalidOperationException("Something went wrong");

            var factoryMock = new Mock<IProgramRunnerFactory>();
            factoryMock
                .Setup(f => f.Create("config.json", loggerMock.Object))
                .Throws(exception);

            var bootstrapper = new Bootstrapper(factoryMock.Object);

            // Act
            bootstrapper.Run(args, loggerMock.Object);

            // Assert
            factoryMock.Verify(f => f.Create("config.json", loggerMock.Object), Times.Once);
            loggerMock.Verify(l => l.Fatal($"Bootstrapper.Run: {exception.Message}"), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldStoreFactory()
        {
            // Arrange
            var factoryMock = new Mock<IProgramRunnerFactory>();

            // Act
            var bootstrapper = new Bootstrapper(factoryMock.Object);

            // Assert
            bootstrapper.Should().NotBeNull();
        }
    }
}

