using EcpSigner.Domain.Interfaces;

namespace EcpSigner
{
    public interface IBootstrapper
    {
        void Run(string[] args, ILogger logger);
    }
}