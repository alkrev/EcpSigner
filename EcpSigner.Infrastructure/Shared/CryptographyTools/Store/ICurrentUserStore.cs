using EcpSigner.Domain.Interfaces;
using System.Collections.Generic;

namespace CryptographyTools.Store
{
    public interface ICurrentUserStore
    {
        Dictionary<string, ICertificate> GetUserCertificates();
    }
}
