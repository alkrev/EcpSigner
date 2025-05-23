using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Decorators
{
    public class SignDocumentsLoopDecorator : ISignDocumentsLoop
    {
        private readonly ISignDocumentsLoop _inner;
        private readonly ILogger _logger;
        private readonly ICacheService _cache;
        public SignDocumentsLoopDecorator(ISignDocumentsLoop inner, ICacheService cache, ILogger logger, IDelayProvider delayProvider)
        {
            _logger = logger;
            _inner = inner;
            _cache = cache;
        }
        public async Task<(int signedCount, List<string> docsToCache)> RunAsync(List<Document> filteredDocs, List<(EcpCertificate, ICertificate)> certs, CancellationToken cancellationToken)
        {
            _logger.Info("подписываем документы");
            var startTime = DateTime.UtcNow;
            (int signedCount, List<string> docsToCache) = await _inner.RunAsync(filteredDocs, certs, cancellationToken);
            _cache.SetRange(docsToCache); // Кешируем документы, которые возвращают ошибку при подписании
            var stopTime = DateTime.UtcNow;
            var elapsedTime = stopTime - startTime;
            _logger.Info($"подписано документов [{signedCount}] за {elapsedTime.TotalSeconds:f} секунд. Кеш: {_cache.Count()}");
            return (signedCount, docsToCache);
        }
    }
}
