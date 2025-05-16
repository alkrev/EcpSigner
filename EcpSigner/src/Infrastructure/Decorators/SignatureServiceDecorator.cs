using CAPICOM;
using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Decorators
{
    public class SignatureServiceDecorator : ISignatureService
    {
        private readonly ISignatureService _inner;
        private readonly ILogger _logger;

        public SignatureServiceDecorator(ISignatureService inner, ILogger logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public Dictionary<string, ICertificate> GetUserCertificates()
        {
            _logger.Debug("получаем список сертификатов пользователя");
            DateTime startTime = DateTime.UtcNow;
            var certs = _inner.GetUserCertificates();
            DateTime stopTime = DateTime.UtcNow;
            var elapsedTime = stopTime - startTime;
            _logger.Debug($"получено сертификатов пользователя {certs.Count} за {elapsedTime.TotalSeconds:f} секунд");
            return certs;
        }
        public string Sign(ICertificate certificate, string docBase64, string document)
        {
            string signature = _inner.Sign(certificate, docBase64, document);
            _logger.Debug($"подпись {document} создана");
            return signature;
        }
    }
}
