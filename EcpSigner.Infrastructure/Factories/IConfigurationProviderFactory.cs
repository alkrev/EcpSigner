using EcpSigner.Domain.Interfaces;

namespace EcpSigner.Infrastructure.Factories
{
    public interface IConfigurationProviderFactory
    {
        IConfigurationProvider Create();
    }
}
