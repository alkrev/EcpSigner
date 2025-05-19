using CAPICOM;
using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Interfaces
{
    public interface IPortalService
    {
        Task Login(string login, string password, CancellationToken cancellationToken);
        Task<List<Document>> SearchDocuments(string startDate, string endDate, CancellationToken token);
        //Task<bool> saveEMDSignatures (string id, string signature, CancellationToken cancellationToken);
        Task<List<EcpCertificate>> LoadEcpCertificates(CancellationToken cancellationToken);
        Task CheckBeforeSign(Document doc, EcpCertificate ecpCert, string docName, CancellationToken cancellationToken);
        Task<(string docBase64, string hashBase64)> GetSignData(Document doc, EcpCertificate ecpCert, string docName, CancellationToken cancellationToken);
        Task SaveSignature(Document doc, string hashBase64, string signature, EcpCertificate ecpCert, string docName, CancellationToken cancellationToken);
    }
}
