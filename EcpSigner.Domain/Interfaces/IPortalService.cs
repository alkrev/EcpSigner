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
        Task Login(string login, string password);
        Task<List<Document>> SearchDocuments(string startDate, string endDate, CancellationToken token);
        Task<List<EcpCertificate>> LoadEcpCertificates();
        Task CheckBeforeSign(Document doc, EcpCertificate ecpCert, string docName);
        Task<List<ToSign>> GetSignData(Document doc, EcpCertificate ecpCert, string docName);
        Task SaveSignature(Document doc, string VersionID, string hashBase64, string signature, EcpCertificate ecpCert, string docName);
    }
}
