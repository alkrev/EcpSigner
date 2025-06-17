using CryptographyTools.Store;

namespace EcpSigner.Infrastructure.Factories
{
    public interface IStoreFactory
    {
        ICurrentUserStore Create();
    }
}
