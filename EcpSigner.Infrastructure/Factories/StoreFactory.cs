using CryptographyTools.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
