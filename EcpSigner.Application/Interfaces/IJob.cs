using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Interfaces
{
    public interface IJob
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
