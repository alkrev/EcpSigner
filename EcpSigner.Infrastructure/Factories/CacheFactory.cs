using CachingTools;

namespace EcpSigner.Infrastructure.Factories
{
    public class CacheFactory : ICacheFactory
    {
        public ICache Create(int minutes)
        {
            return new Cache(minutes);
        }
    }
}
