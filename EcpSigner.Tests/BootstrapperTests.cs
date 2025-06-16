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
        public void Run_ShouldCallRunnerRunAsync_WhenNoExceptionOccurs()
        {
            // Arrange
            var args = new[] { "arg1", "arg2" };
            var runnerMock = new Mock<IProgramRunner>();
            runnerMock.Setup(r => r.RunAsync(args)).Returns(Task.CompletedTask);

            var factoryMock = new Mock<IProgramRunnerFactory>();
            factoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<ILogger>()))
                       .Returns(runnerMock.Object);

            var loggerMock = new Mock<ILogger>();

            var bootstrapper = new Bootstrapper(factoryMock.Object, loggerMock.Object);

            // Act
            bootstrapper.Run(args);

            // Assert
            factoryMock.Verify(f => f.Create("config.json", It.IsAny<ILogger>()), Times.Once);
            runnerMock.Verify(r => r.RunAsync(args), Times.Once);
        }

        [Fact]
        public void Run_ShouldLogFatal_WhenRunnerThrowsException()
        {
            // Arrange
            var args = new[] { "arg1" };
            var exception = new InvalidOperationException("test exception");

            var runnerMock = new Mock<IProgramRunner>();
            runnerMock.Setup(r => r.RunAsync(args)).ThrowsAsync(exception);

            var loggerMock = new Mock<ILogger>();
            var factoryMock = new Mock<IProgramRunnerFactory>();
            factoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<ILogger>()))
                .Returns(runnerMock.Object);

            var bootstrapper = new Bootstrapper(factoryMock.Object, loggerMock.Object);

            // Act
            bootstrapper.Run(args);

            // Assert
            loggerMock.Verify(l => l.Fatal(It.Is<string>(s => s.Contains("Bootstrapper.Run") && s.Contains("test exception"))), Times.Once);
        }
    }
}

