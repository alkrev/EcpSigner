using CAdESCOM;
using CAPICOM;
using CryptographyTools.Signing;
using CryptographyTools.Signing.CryptoPro;

namespace EcpSigner.Infrastructure.Factories
{
    public class CryptoFactory : ICryptoFactory
    {
        public ISigning Create()
        {
            var signer = new CPSigner();
            var signedData = new CadesSignedData();
            return new Crypto(signer, signedData);
        }
    }
}
