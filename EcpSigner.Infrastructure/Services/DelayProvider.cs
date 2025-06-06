using EcpSigner.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Services
{
    public class DelayProvider : IDelayProvider
    {
        public async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Исключение подавлено
            }
        }
    }
}
