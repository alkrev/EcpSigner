using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public interface IProgramRunner
    {
        Task RunAsync(string[] args);
    }
}
