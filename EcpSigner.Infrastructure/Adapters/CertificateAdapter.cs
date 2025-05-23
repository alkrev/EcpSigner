using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Adapters
{
    public class CertificateAdapter : ICertificate
    {
        private readonly CAPICOM.ICertificate _comCert;

        public CertificateAdapter(CAPICOM.ICertificate comCert)
        {
            _comCert = comCert;
        }
        public string Subject => _comCert.SubjectName;
        public DateTime ValidToDate => _comCert.ValidToDate;

        public CAPICOM.ICertificate GetCertificate => _comCert;
    }
}
