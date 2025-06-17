using EcpSigner.Domain.Interfaces;

namespace EcpSigner.Infrastructure.Factories
{
    public interface IProgramRunnerFactory
    {
        IProgramRunner Create(string configPath, ILogger logger);
    }
}
