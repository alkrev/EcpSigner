using CachingTools;
using EcpSigner.Domain.Interfaces;

namespace EcpSigner.Infrastructure.Factories
{
    public class CacheFactory : ICacheFactory
    {
        public ICache Create(int minutes, IDateTimeProvider dateTimeProvider)
        {
            return new Cache(minutes, dateTimeProvider);
        }
    }
}
