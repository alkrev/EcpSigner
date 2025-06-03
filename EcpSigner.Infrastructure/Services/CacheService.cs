using EcpSigner.Domain.Interfaces;
using System.Collections.Generic;

namespace EcpSigner.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        CachingTools.ICache _cache;
        public CacheService(CachingTools.ICache cache)
        {
            _cache = cache;
        }
        public bool Contains(string number) => _cache.Contains(number);
        public int Count() => _cache.Count();
        public void RemoveExpired() => _cache.RemoveExpired();
        public void SetRange(List<string> numbers) => _cache.SetRange(numbers);
    }
}
