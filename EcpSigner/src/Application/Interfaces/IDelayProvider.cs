using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Interfaces
{
    public interface IDelayProvider
    {
        Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
    }
}
