using CAdESCOM;
using CryptographyTools.Signing;
using CryptographyTools.Store;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Adapters;
using System;
using System.Collections.Generic;

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
        public string Sign(ICertificate certificate, string docBase64, string document, string versionID)
        {
            if (!(certificate is CertificateAdapter))
                throw new ArgumentException("неверный тип сертификата");
            var certAdapter = (CertificateAdapter)certificate;
            var comCert = certAdapter.GetCertificate;
            return _signing.Sign(comCert, docBase64);
        }
    }
}
