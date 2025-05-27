using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Application.Jobs
{
    public class DocumentSigningJobTests
    {
        private readonly Mock<IJob> _mockWorkflow;
        private readonly Mock<ILogger> _mockLogger;
        private readonly DocumentSigningJob _job;

        public DocumentSigningJobTests()
        {
            _mockWorkflow = new Mock<IJob>();
            _mockLogger = new Mock<ILogger>();
            _job = new DocumentSigningJob(_mockWorkflow.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RunAsync_Should_CompleteSuccessfully_When_NoExceptions()
        {
            // Act
            Func<Task> act = async () => await _job.RunAsync(CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _mockWorkflow.Verify(x => x.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_LogFatalAndThrow_When_BreakWorkExceptionThrown()
        {
            // Arrange
            _mockWorkflow
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new BreakWorkException("breaking"));

            // Act
            Func<Task> act = async () => await _job.RunAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("DocumentSigningJob: breaking");

            _mockLogger.Verify(x => x.Fatal("DocumentSigningJob: breaking"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_LogInfo_When_StopWorkExceptionThrown()
        {
            // Arrange
            _mockWorkflow
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new StopWorkException());

            // Act
            await _job.RunAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(x => x.Info("остановка работы"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_LogWarn_When_IsNotLoggedInExceptionThrown()
        {
            // Arrange
            _mockWorkflow
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IsNotLoggedInException("вход не выполнен"));

            // Act
            await _job.RunAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(x => x.Warn("вход не выполнен"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_LogDebug_When_ContinueExceptionThrown()
        {
            // Arrange
            var message = "continue";
            _mockWorkflow
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ContinueException(message));

            // Act
            await _job.RunAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(x => x.Debug(message), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_LogError_When_ContinueExceptionWithErrorThrown()
        {
            // Arrange
            var message = "error occurred";
            _mockWorkflow
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ContinueExceptionWithError(message));

            // Act
            await _job.RunAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(x => x.Error($"DocumentSigningJob: {message}"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_LogErrorAndDebug_When_UnexpectedExceptionThrown()
        {
            // Arrange
            var exception = new InvalidOperationException("unexpected");
            _mockWorkflow
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await _job.RunAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(x => x.Error("DocumentSigningJob: unexpected"), Times.Once);
            _mockLogger.Verify(x => x.Debug($"DocumentSigningJob: {exception.GetType().Name}"), Times.Once);
            _mockLogger.Verify(x => x.Debug(It.Is<string>(s => s.StartsWith("DocumentSigningJob:"))), Times.AtLeast(1));
        }
    }
}
