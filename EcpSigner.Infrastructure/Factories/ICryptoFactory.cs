using CryptographyTools.Signing;

namespace EcpSigner.Infrastructure.Factories
{
    public interface ICryptoFactory
    {
        ISigning Create();
    }
}
