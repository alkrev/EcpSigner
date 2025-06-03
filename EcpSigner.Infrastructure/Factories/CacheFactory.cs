using CachingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
