using CAPICOM;
using EcpSigner.Application.Filters;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentSigner.Application.Jobs
{
    public class PrepareSigningWorkflow : IJob
    {
        private readonly IPortalService _repository;
        private readonly ISignatureService _signatureService;
        private readonly ILogger _logger;
        private readonly IConfigurationProvider _config;
        private readonly IDatesService _dates;
        private readonly ICache _cache;
        private readonly IFlashWindowService _flashWindowService;
        private readonly ISignDocumentsLoop _signDocumentsLoop;

        private bool isLoggedOn;
        private DateTime lastCheck;

        public PrepareSigningWorkflow(
            IPortalService repository,
            ISignatureService signatureService,
            ILogger logger, 
            IConfigurationProvider config,
            IDatesService dates,
            ICache cache,
            IFlashWindowService flashWindowService,
            ISignDocumentsLoop signDocumentsLoop
        )
        {
            _repository = repository;
            _signatureService = signatureService;
            _logger = logger;
            _config = config;
            _dates = dates;
            _cache = cache;
            _flashWindowService = flashWindowService;
            _signDocumentsLoop = signDocumentsLoop;

            isLoggedOn = false;
            lastCheck = DateTime.Now;
        }
        /// <summary>
        /// Подписываем документы
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                CacheRemoveExpired(ref lastCheck);
                await TryLogin(cancellationToken);
                (string startDate, string endDate) = _dates.GetDates();
                var docs = await GetDocuments(startDate, endDate, cancellationToken);
                ShowDocumentsWithError(docs);
                ShowIgnoredDocuments(docs);
                FlashWindowIfThereAreIgnoredDocuments(docs);
                var filteredDocs = FilterDocuments(docs);
                var certs = await GetCertificatesWithTime(cancellationToken);
                await SignDocumentsLoop(filteredDocs, certs, cancellationToken);
                LoggerFlush();
            }
            catch (IsNotLoggedInException)
            {
                isLoggedOn = false;
                throw;
            }
        }
        /// <summary>
        /// Сохраняем лог
        /// </summary>
        private void LoggerFlush()
        {
            _logger.Flush();
        }
        /// <summary>
        /// Получаем подходящие для подписи сертификаты c замеров времени
        /// </summary>
        private async Task<List<(EcpCertificate, ICertificate)>> GetCertificatesWithTime(CancellationToken cancellationToken)
        {
            _logger.Info("получаем список сертификатов");
            DateTime startTime = DateTime.UtcNow;
            var certs = await GetCertificates(cancellationToken);
            DateTime stopTime = DateTime.UtcNow;
            var elapsedTime = stopTime - startTime;
            _logger.Info($"получено сертификатов {certs.Count} за {elapsedTime.TotalSeconds:f} секунд");
            return certs;
        }
        /// <summary>
        /// Получаем подходящие для подписи сертификаты
        /// </summary>
        private async Task<List<(EcpCertificate, ICertificate)>> GetCertificates(CancellationToken cancellationToken)
        {
            var ecpCerts = await LoadEcpCertificates(cancellationToken);
            var userCerts = GetUserCertificates();
            var matchedCerts = GetMatchedCertificates(ecpCerts, userCerts);
            var suitableCerts = GetSuitableCertificates(matchedCerts);
            return suitableCerts;
        }
        /// <summary>
        /// Проверяем сертификаты, для новых сертифкатов запрашивается пинкод
        /// </summary>
        private List<(EcpCertificate, ICertificate)> GetSuitableCertificates(List<(EcpCertificate, ICertificate)> certs)
        {
            List<(EcpCertificate, ICertificate)> suitableCerts = new List<(EcpCertificate, ICertificate)>();
            foreach ((EcpCertificate, ICertificate) cert in certs)
            {
                try
                {
                    _signatureService.Sign(cert.Item2, "test", "test");
                    suitableCerts.Add(cert);
                }
                catch { }
            }
            if (suitableCerts.Count == 0)
            {
                throw new BreakWorkException("У пользователя в операционной системе подходящие сертификаты не найдены");
            }
            return suitableCerts;
        }
        /// <summary>
        /// Ищем сертификаты в хранилище пользователя, соответствующие сертификатам в ЕЦП
        /// </summary>
        private List<(EcpCertificate, ICertificate)> GetMatchedCertificates(List<EcpCertificate> ecpCerts, Dictionary<string, ICertificate> userCerts)
        {
            List<(EcpCertificate, ICertificate)> matchedCerts = new List<(EcpCertificate, ICertificate)>();
            foreach (EcpCertificate ecpCert in ecpCerts)
            {
                if (userCerts.ContainsKey(ecpCert.thumbprint))
                {
                    matchedCerts.Add((ecpCert, userCerts[ecpCert.thumbprint]));
                }
            }
            if (matchedCerts.Count == 0)
            {
                throw new BreakWorkException("у пользователя в операционной системе сертификаты не найдены");
            }
            return matchedCerts;
        }
        /// <summary>
        /// Запускаем задачу подписания документов
        /// </summary>
        private async Task<(int signedCount, List<string> docsToCache)> SignDocumentsLoop(List<Document> filteredDocs, List<(EcpCertificate, ICertificate)> certs, CancellationToken cancellationToken)
        {
             return await _signDocumentsLoop.RunAsync(filteredDocs, certs, cancellationToken);
        }
        /// <summary>
        /// Получаем список сертификатов
        /// </summary>
        private Dictionary<string, CAPICOM.ICertificate> GetUserCertificates()
        {
            Dictionary<string, CAPICOM.ICertificate> certs = _signatureService.GetUserCertificates();
            return certs;
        }
        /// <summary>
        /// Загружаем список сертификатов пользователя из ЕЦП
        /// </summary>
        private async Task<List<EcpCertificate>> LoadEcpCertificates(CancellationToken cancellationToken)
        {
            List<EcpCertificate> ecpCerts = await _repository.LoadEcpCertificates();
            if (ecpCerts.Count == 0) throw new BreakWorkException("у пользователя ECP не обнаружены сертификаты");
            return ecpCerts;
        }
        /// <summary>
        /// Убираем документы, которые не будем пытаться подписывать
        /// </summary>
        private List<Document> FilterDocuments(List<Document> docs)
        {
            List<Document> filteredDocs = docs
                .FindAll(doc => DocumentFilters.NeedsSigning(doc))  // Требуется подпись и нет ошибок
                .FindAll(doc => !DocumentFilters.IgnoredDocument(doc, _config.Get().ignoreDocTypesDict)) // Документ не игнорируется по типу
                .FindAll(doc => !_cache.Contains(doc.ID)); // Документ не содержится в кеше
            _logger.Info(string.Format("выбрано для отправки документов {0}", filteredDocs.Count));
            if (filteredDocs.Count == 0) throw new ContinueException("не найдены документы для отправки");
            return filteredDocs;
        }
        /// <summary>
        /// Мигаем окном если есть документы, игнорируемые по типу
        /// </summary>
        private void FlashWindowIfThereAreIgnoredDocuments(List<Document> docs)
        {
            if (docs.Exists(doc => DocumentFilters.IgnoredDocument(doc, _config.Get().ignoreDocTypesDict)))
            {
                _flashWindowService.Start();
            }
            else
            {
                _flashWindowService.Stop();
            }
        }
        /// <summary>
        /// Показываем документы, которые игнорируются по типу
        /// </summary>
        private void ShowIgnoredDocuments(List<Document> docs)
        {
            docs.FindAll(doc => DocumentFilters.IgnoredDocument(doc, _config.Get().ignoreDocTypesDict)).ForEach(doc =>
            {
                _logger.Warn(string.Format("'{0} - {1} ({2})' следует подписывать вручную", doc.Name, doc.Num, doc.VersionNumber));
            });
        }
        /// <summary>
        /// Показываем документы с ошибками
        /// </summary>
        private void ShowDocumentsWithError(List<Document> docs)
        {
            docs.FindAll(doc => DocumentFilters.WithError(doc)).ForEach(doc =>
            {
                _logger.Info(string.Format("'{0} - {1} ({2})': {3}", doc.Name, doc.Num, doc.VersionNumber, doc.Error));
            });
        }
        /// <summary>
        /// Выполняем вход, если вход ещё не выполнен
        /// </summary>
        private async Task TryLogin(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) throw new StopWorkException();
            if (!isLoggedOn)
            {
                await _repository.Login(_config.Get().login, _config.Get().password);
                isLoggedOn = true;
            }
        }
        /// <summary>
        /// Удаляем просроченные документы из кеша
        /// </summary>
        private void CacheRemoveExpired(ref DateTime lastCheck)
        {
            DateTime now = DateTime.Now;
            if (lastCheck.Day != now.Day)
            {
                _logger.Info("очищаем кеш");
                _cache.RemoveExpired();
                lastCheck = now;
            }
        }
        /// <summary>
        /// Получаем документы
        /// </summary>
        private async Task<List<Document>> GetDocuments(string startDate, string endDate, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) throw new StopWorkException();
            return await _repository.SearchDocuments(startDate, endDate, cancellationToken);
        }
    }
}