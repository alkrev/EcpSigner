using CAPICOM;
using CryptographyTools.Signing;
using CryptographyTools.Store;
using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Repositories
{
    public class SignatureService : ISignatureService
    {
        private readonly ISigning _signing;
        private readonly ICurrentUserStore _store;
        public SignatureService(ISigning crypto, ICurrentUserStore store)
        {
            _signing = crypto;
            _store = store;
        }
        public Dictionary<string, ICertificate> GetUserCertificates()
        {
            return _store.GetUserCertificates();
        }
        public string Sign(ICertificate certificate, string docBase64, string document)
        {
            return _signing.Sign(certificate, docBase64);
        }
    }
}
