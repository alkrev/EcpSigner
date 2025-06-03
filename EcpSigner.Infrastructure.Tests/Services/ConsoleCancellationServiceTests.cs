using ConsoleTools;
using EcpSigner.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EcpSigner.Infrastructure.Services
{
    public class ConsoleCancellationServiceTests
    {
        [Fact]
        public void HandleCancelKeyPress_ShouldCancelToken_AndLogInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var consoleMock = new Mock<IConsoleWrapper>();
            var service = new ConsoleCancellationService(loggerMock.Object, consoleMock.Object);

            var tokenCancelled = false;
            service.Token.Register(() => tokenCancelled = true);

            // Мокаем internal ConsoleCancelEventArgs через рефлексию (см. ниже для пояснения)
            var ctor = typeof(ConsoleCancelEventArgs)
                .GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(ConsoleSpecialKey) }, null)!;
            var cancelArgs = (ConsoleCancelEventArgs)ctor.Invoke(new object[] { ConsoleSpecialKey.ControlC });

            // Act
            service.StartListeningForCancel();
            service.HandleCancelKeyPress(this, cancelArgs);

            // Assert
            cancelArgs.Cancel.Should().BeTrue();
            tokenCancelled.Should().BeTrue();
            loggerMock.Verify(l => l.Info("Ctrl-C нажато"), Times.Once);
        }
    }
}
