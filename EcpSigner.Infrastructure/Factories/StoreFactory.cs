using CryptographyTools.Store;

namespace EcpSigner.Infrastructure.Factories
{
    public class StoreFactory : IStoreFactory
    {
        public ICurrentUserStore Create()
        {
            return new CurrentUserStore();
        }
    }
}
