using CryptographyTools.Signing;
using CryptographyTools.Signing.CryptoPro;

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
