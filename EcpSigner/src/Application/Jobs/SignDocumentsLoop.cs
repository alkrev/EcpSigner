using CAPICOM;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Jobs
{
    public class SignDocumentsLoop : ISignDocumentsLoop
    {
        private readonly ILogger _logger;
        private readonly ISignDocumentWorflow _signDocumentsWorflow;
        private readonly IConfigurationProvider _config;
        private readonly IDelayProvider _delayProvider;
        public SignDocumentsLoop(ILogger logger, IConfigurationProvider config, ISignDocumentWorflow signDocumentsWorflow, IDelayProvider delayProvider)
        {
            _logger = logger;
            _signDocumentsWorflow = signDocumentsWorflow;
            _config = config;
            _delayProvider = delayProvider;
        }
        public async Task<(int signedCount, List<string> docsToCache)> RunAsync(List<Document> docs, List<(EcpCertificate, ICertificate)> certs, CancellationToken cancellationToken)
        {
            int count = 0;
            List<string> errorDocNums = new List<string>();
            foreach (Document doc in docs)
            {
                if (cancellationToken.IsCancellationRequested) throw new StopWorkException();
                string document = string.Format("'{0} - {1} ({2})'", doc.Name, doc.Num, doc.VersionNumber);
                try
                {
                    await _signDocumentsWorflow.RunAsync(doc, certs, cancellationToken);
                    count++;
                }
                catch (DocumentSigningException ex)
                {
                    _logger.Warn($"{document}: {ex.Message ?? "SignDocs: warning"}");
                    errorDocNums.Add(doc.ID);
                }
                catch (IsNotLoggedInException)
                {
                    throw;
                }
                catch (StopWorkException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error($"SignDocumentsLoop: {document}: {ex.Message ?? "ошибка"}");
                    break;
                }
                await _delayProvider.DelayAsync(TimeSpan.FromSeconds(_config.Get().pauseMinutes), cancellationToken);
            }
            return (count, errorDocNums);
        }
    }
}
