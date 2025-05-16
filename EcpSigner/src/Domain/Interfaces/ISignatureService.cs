using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Interfaces
{
    public interface ISignatureService
    {
        string Sign(CAPICOM.ICertificate certificate, string docBase64, string document);
        Dictionary<string, CAPICOM.ICertificate> GetUserCertificates();
    }
}