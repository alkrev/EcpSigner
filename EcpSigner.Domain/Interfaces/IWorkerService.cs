using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Interfaces
{
    public interface IWorkerService
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
