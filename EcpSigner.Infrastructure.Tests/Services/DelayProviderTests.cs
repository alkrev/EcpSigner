using System;
using System.Threading;
using System.Threading.Tasks;
using EcpSigner.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace EcpSigner.Infrastructure.Services
{
    public class DelayProviderTests
    {
        [Fact]
        public async Task DelayAsync_ShouldDelayWithoutCancellation()
        {
            // Arrange
            var delayProvider = new DelayProvider();
            var delay = TimeSpan.FromMilliseconds(100);
            var cancellationToken = CancellationToken.None;

            var before = DateTime.UtcNow;

            // Act
            await delayProvider.DelayAsync(delay, cancellationToken);

            var after = DateTime.UtcNow;

            // Assert (допуск 100 мс для надёжности)
            (after - before).Should().BeCloseTo(delay, TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public async Task DelayAsync_ShouldSuppressTaskCanceledException()
        {
            // Arrange
            var delayProvider = new DelayProvider();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var delay = TimeSpan.FromSeconds(1); // большой, чтобы наверняка

            Func<Task> act = async () => await delayProvider.DelayAsync(delay, cts.Token);

            // Act & Assert
            await act.Should().NotThrowAsync<TaskCanceledException>();
        }
    }
}
