using Castle.Core.Configuration;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Workers
{
    public class DocumentSigningWorkerTests
    {
        private readonly Mock<IJob> _jobMock = new();
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IConfigurationProvider> _configProviderMock = new();
        private readonly Mock<IAppTitleService> _appTitleServiceMock = new();
        private readonly Mock<IDelayProvider> _delayProviderMock = new();
        private readonly DocumentSigningWorker _worker;

        public DocumentSigningWorkerTests()
        {
            var config = new AppSettings();
            config.pauseMinutes = 1;
            _configProviderMock.Setup(c => c.Get()).Returns(config);

            _worker = new DocumentSigningWorker(
                _jobMock.Object,
                _loggerMock.Object,
                _configProviderMock.Object,
                _appTitleServiceMock.Object,
                _delayProviderMock.Object
            );
        }

        [Fact]
        public async Task RunAsync_Should_Call_AppTitleService_And_Run_Job_And_Delay()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(100); // прерывание цикла после одной итерации

            _delayProviderMock
                .Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _jobMock
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _worker.RunAsync(cts.Token);

            // Assert
            _appTitleServiceMock.Verify(x => x.Set(), Times.Once);
            _jobMock.Verify(x => x.RunAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _delayProviderMock.Verify(x => x.DelayAsync(TimeSpan.FromMinutes(1), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _loggerMock.Verify(x => x.Info("работа завершена"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Log_Debug_When_Exception_Thrown()
        {
            // Arrange
            var exception = new InvalidOperationException("test error");

            _jobMock
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await _worker.RunAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.Debug("остановка: test error"), Times.Once);
            _loggerMock.Verify(x => x.Info("работа завершена"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Log_Default_Debug_Message_When_Exception_Without_Message()
        {
            // Arrange
            var exception = new Exception("тестовое исключение");

            _jobMock
                .Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await _worker.RunAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.Debug("остановка: тестовое исключение"), Times.Once);
            _loggerMock.Verify(x => x.Info("работа завершена"), Times.Once);
        }
    }
}
