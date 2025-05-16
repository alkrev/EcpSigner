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
        public SignDocumentsLoop(ILogger logger, IConfigurationProvider config, ISignDocumentWorflow signDocumentsWorflow)
        {
            _logger = logger;
            _signDocumentsWorflow = signDocumentsWorflow;
            _config = config;
        }
        public async Task<(int signedCount, List<string> docsToCache)> RunAsync(List<Document> docs, List<(EcpCertificate, ICertificate)> certs, CancellationToken cancellationToken)
        {
            int count = 0;
            List<string> errorDocNums = new List<string>();
            foreach (Document doc in docs)
            {
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
                    _logger.Error($"{document}: {ex.Message ?? "SignDocumentsLoop: ошибка"}");
                    break;
                }
                for (int i = 0; i < _config.Get().signingIntervalSeconds; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(1 * 1000);
                }
            }
            return (count, errorDocNums);
        }
    }
}
