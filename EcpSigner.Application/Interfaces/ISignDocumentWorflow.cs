using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Interfaces
{
    public interface ISignDocumentWorflow
    {
        Task RunAsync(Document doc, List<(EcpCertificate, ICertificate)> certs, CancellationToken cancellationToken);
    }
}
