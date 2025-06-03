using CachingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public interface ICacheFactory
    {
        ICache Create(int minutes);
    }
}
