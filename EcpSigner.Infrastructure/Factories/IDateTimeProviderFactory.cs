using EcpSigner.Domain.Interfaces;

namespace EcpSigner.Infrastructure.Factories
{
    public interface IDateTimeProviderFactory
    {
        IDateTimeProvider Create();
    }
}
