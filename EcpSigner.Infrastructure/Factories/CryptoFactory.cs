using CryptographyTools.Signing;
using CryptographyTools.Signing.CryptoPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public class CryptoFactory : ICryptoFactory
    {
        public ISigning Create()
        {
            return new Crypto();
        }
    }
}
