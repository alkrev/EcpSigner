using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Interfaces
{
    public interface IWorkerFactory
    {
        IJob CreateWorker(string[] args);
    }
}
