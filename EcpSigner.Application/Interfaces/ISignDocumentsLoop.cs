using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Interfaces
{
    public interface ISignDocumentsLoop
    {
        Task<(int signedCount, List<string> docsToCache)>  RunAsync(List<Document> filteredDocs, List<(EcpCertificate, ICertificate)> certs, CancellationToken cancellationToken);
    }
}
