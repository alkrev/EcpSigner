using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Jobs
{
    public class SignDocumentWorflow : ISignDocumentWorflow
    {
        private readonly IPortalService _repository;
        private readonly ISignatureService _signatureService;
        private readonly ILogger _logger;
        public SignDocumentWorflow
        (
            IPortalService repository,
            ISignatureService signatureService,
            ILogger logger
        )
        {
            _repository = repository;
            _signatureService = signatureService;
            _logger = logger;
        }
        public async Task RunAsync(Document doc, List<(EcpCertificate, ICertificate)> certs, CancellationToken cancellationToken)
        {
            string docName = string.Format("'{0} - {1} ({2})'", doc.Name, doc.Num, doc.VersionNumber);
            (EcpCertificate ecpCert, ICertificate userCert) = SelectCertificate(certs, cancellationToken);
            await CheckBeforeSign(doc, ecpCert, docName, cancellationToken);
            List<ToSign> toSigns = await GetSignData(doc, ecpCert, docName, cancellationToken);
            foreach (ToSign toSign in toSigns)
            {
                string signature = Sign(toSign.docBase64, userCert, docName, toSign.EMDVersion_id, cancellationToken);
                await SaveSignature(doc, toSign.EMDVersion_id, ecpCert, signature, toSign.hashBase64, docName, cancellationToken);
            }
        }
        /// <summary>
        /// Сохранение подписи документа
        /// </summary>
        /// <returns></returns>
        private async Task SaveSignature(Document doc, string versionID, EcpCertificate ecpCert, string signature, string hashBase64, string docName, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) throw new StopWorkException();
            await _repository.SaveSignature(doc, versionID, hashBase64, signature, ecpCert, docName);
        }
        /// <summary>
        /// Вычисляем подпись
        /// </summary>
        private string Sign(string docBase64, ICertificate userCert, string docName, string versionID, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) throw new StopWorkException();
            string signature = _signatureService.Sign(userCert, docBase64, docName, versionID);
            return signature;
        }
        /// <summary>
        /// Получаем документ и хеш для подписания
        /// </summary>
        private async Task<List<ToSign>> GetSignData(Document doc, EcpCertificate ecpCert, string docName, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) throw new StopWorkException();
            return await _repository.GetSignData(doc, ecpCert, docName);
        }
        /// <summary>
        /// Выбираем валидный сертификат
        /// </summary>
        private (EcpCertificate ecpCert, ICertificate userCert) SelectCertificate(List<(EcpCertificate, ICertificate)> certs, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) throw new StopWorkException();
            DateTime now = DateTime.Now;
            foreach ((EcpCertificate, ICertificate) cert in certs)
            {
                if (now + TimeSpan.FromSeconds(1) < cert.Item2.ValidToDate)
                {
                    _logger.Debug(string.Format("сертификат выбран: {0} срок действия {1}", cert.Item2.Subject, cert.Item2.ValidToDate.ToString("dd.MM.yyyy HH:mm:ss")));
                    return (cert.Item1, cert.Item2);
                }
                else
                {
                    _logger.Warn(string.Format("сертификат невалидный: {0} срок действия {1}", cert.Item2.Subject, cert.Item2.ValidToDate.ToString("dd.MM.yyyy HH:mm:ss")));
                }
            }
            throw new Exception("подходящие сертификаты не найдены");
        }
        /// <summary>
        /// Проверка документа перед подписанием
        /// </summary>
        private async Task CheckBeforeSign(Document doc, EcpCertificate ecpCert, string docName, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) throw new StopWorkException();
            await _repository.CheckBeforeSign(doc, ecpCert, docName);
        }
    }
}
