using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Decorators
{
    public class PortalServiceDecorator : IPortalService
    {
        private readonly IPortalService _inner;
        private readonly ILogger _logger;

        public PortalServiceDecorator(IPortalService inner, ILogger logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task Login(string login, string password)
        {
            _logger.Info("выполняем вход");
            await _inner.Login(login, password);
            _logger.Info("вход выполнен");
        }
        public async Task<List<Document>> SearchDocuments(string startDate, string endDate, CancellationToken token)
        {
            _logger.Info($"получаем список документов {startDate}-{endDate}");
            DateTime startTime = DateTime.UtcNow;
            List<Document> docs = await _inner.SearchDocuments(startDate, endDate, token);
            DateTime stopTime = DateTime.UtcNow;
            var elapsedTime = stopTime - startTime;
            _logger.Info($"получено документов {docs.Count} за {elapsedTime.TotalSeconds:f} секунд");
            return docs;
        }
        public async Task CheckBeforeSign(Document doc, EcpCertificate ecpCert, string docName)
        {
            await _inner.CheckBeforeSign(doc, ecpCert, docName);
            _logger.Debug(string.Format("проверка перед подписанием документа {0} прошла успешно", docName));
        }
        public async Task<(string docBase64, string hashBase64)> GetSignData(Document doc, EcpCertificate ecpCert, string docName)
        {
            (string docBase64, string hashBase64) = await _inner.GetSignData(doc, ecpCert, docName);
            _logger.Debug(string.Format("получение документа для подписания {0} прошло успешно", docName));
            return (docBase64, hashBase64);
        }
        public async Task<List<EcpCertificate>> LoadEcpCertificates()
        {
            _logger.Info("получаем список сертификатов");
            DateTime startTime = DateTime.UtcNow;
            List<EcpCertificate> certs = await _inner.LoadEcpCertificates();
            DateTime stopTime = DateTime.UtcNow;
            var elapsedTime = stopTime - startTime;
            _logger.Info($"получено сертификатов {certs.Count} за {elapsedTime.TotalSeconds:f} секунд");
            return certs;
        }
        public async Task SaveSignature(Document doc, string hashBase64, string signature, EcpCertificate ecpCert, string docName)
        {
            await _inner.SaveSignature(doc, hashBase64, signature, ecpCert, docName);
            _logger.Debug(string.Format("подпись документа {0} сохранена на сервере", docName));
        }
    }
}
