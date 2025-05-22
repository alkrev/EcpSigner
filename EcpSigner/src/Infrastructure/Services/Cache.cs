using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Services
{
    class Cache : ICache
    {
        CacheTools.ICache _cache;
        public Cache(CacheTools.ICache cache)
        {
            _cache = cache;
        }
        public bool Contains(string number) => _cache.Contains(number);
        public int Count() => _cache.Count();
        public void RemoveExpired() => _cache.RemoveExpired();
        public void SetRange(List<string> numbers) => _cache.SetRange(numbers);
    }
}
