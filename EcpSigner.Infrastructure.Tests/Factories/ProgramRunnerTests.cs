using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;

namespace EcpSigner.Infrastructure.Factories
{
    public class ProgramRunnerTests
    {
        [Fact]
        public async Task RunAsync_ShouldRunWorker_WithProvidedToken()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var workerMock = new Mock<IJob>();
            var factoryMock = new Mock<IWorkerFactory>();
            var cancelMock = new Mock<ICancellationService>();

            var cts = new CancellationTokenSource();
            cancelMock.SetupGet(c => c.Token).Returns(cts.Token);
            cancelMock.Setup(c => c.StartListeningForCancel());

            factoryMock.Setup(f => f.CreateWorker(It.IsAny<string[]>()))
                       .Returns(workerMock.Object);

            var runner = new ProgramRunner(loggerMock.Object, factoryMock.Object, cancelMock.Object);
            var args = new[] { "test" };

            // Act
            await runner.RunAsync(args);

            // Assert
            cancelMock.Verify(c => c.StartListeningForCancel(), Times.Once);
            factoryMock.Verify(f => f.CreateWorker(args), Times.Once);
            workerMock.Verify(w => w.RunAsync(cts.Token), Times.Once);
        }
        [Fact]
        public async Task RunAsync_ShouldRunWorker_WithoutProvidedToken()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var workerMock = new Mock<IJob>();
            var factoryMock = new Mock<IWorkerFactory>();
            var cancellationService = new Mock<ICancellationService>();

            factoryMock.Setup(f => f.CreateWorker(It.IsAny<string[]>()))
                       .Returns(workerMock.Object);

            cancellationService.Setup(f => f.Token)
                       .Returns(new CancellationToken(false));

            var runner = new ProgramRunner(loggerMock.Object, factoryMock.Object, cancellationService.Object);
            var args = new[] { "test" };

            // Act
            await runner.RunAsync(args);

            // Assert
            factoryMock.Verify(f => f.CreateWorker(args), Times.Once);
            workerMock.Verify(w => w.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Run_ShouldLogFatal_WhenRunnerThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var workerMock = new Mock<IJob>();
            var factoryMock = new Mock<IWorkerFactory>();
            var cancelMock = new Mock<ICancellationService>();

            var cts = new CancellationTokenSource();
            cancelMock.SetupGet(c => c.Token).Returns(cts.Token);
            cancelMock.Setup(c => c.StartListeningForCancel());

            factoryMock.Setup(f => f.CreateWorker(It.IsAny<string[]>()))
                       .Returns(workerMock.Object);
            workerMock.Setup(w => w.RunAsync(It.IsAny<CancellationToken>())).Throws(new Exception("failure"));

            var runner = new ProgramRunner(loggerMock.Object, factoryMock.Object, cancelMock.Object);
            var args = new[] { "test" };

            // Act
            await runner.RunAsync(args);

            // Assert
            loggerMock.Verify(l => l.Fatal(It.Is<string>(msg => msg.Contains("failure"))), Times.Once);
        }
    }
}
