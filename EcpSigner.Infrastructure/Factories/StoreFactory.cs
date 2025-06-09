using CryptographyTools.Store;

namespace EcpSigner.Infrastructure.Factories
{
    public class StoreFactory : IStoreFactory
    {
        public ICurrentUserStore Create()
        {
            var store = new CAPICOM.Store();
            return new CurrentUserStore(store);
        }
    }
}
