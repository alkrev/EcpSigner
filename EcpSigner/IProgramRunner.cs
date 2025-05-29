using System.Threading.Tasks;

namespace EcpSigner
{
    public interface IProgramRunner
    {
        Task RunAsync(string[] args);
    }
}