using Ecp.Portal;
using Ecp.Web;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Repositories
{
    public class PortalService : IPortalService
    {
        private IMain _main;
        private IEMD _emd;
        public PortalService(
            IMain main,
            IEMD emd
        )
        {
            _main = main;
            _emd = emd;
        }
        /// <summary>
        /// Выполняем вход
        /// </summary>
        public async Task Login(string login, string password)
        {
            loginReply rep = await _main.Login(login, password);
            if (!rep.success)
            {
                string err = rep.Error_Msg;
                throw new BreakWorkException(err);
            }
        }
        /// <summary>
        /// Получаем список документов
        /// </summary>
        public async Task<List<Document>> SearchDocuments(string startDate, string endDate, CancellationToken token)
        {
            List<Document> docs;
            try
            {
                docs = new List<Document>();
                int start = 0;
                int count = 30;
                int page = 1;
                while (true)
                {
                    if (token.IsCancellationRequested) throw new StopWorkException();
                    List<Document> rep = await GetDocumentPage(startDate, endDate, start, page, count);
                    if (rep.Count == 0)
                    {
                        break;
                    }
                    docs.AddRange(rep);
                    start += count;
                    page += 1;
                }
            }
            catch (DeserializeException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
            return docs;
        }
        /// <summary>
        /// Получаем "страницу" документов
        /// </summary>
        private async Task<List<Document>> GetDocumentPage(string startDate, string endDate, int start, int page, int count)
        {
            List<loadEMDSignBundleWindowReply> rep = await _emd.loadEMDSignBundleWindow(startDate, endDate, start, page, count);
            List<Document> docs = new List<Document>(rep.Count);
            foreach (loadEMDSignBundleWindowReply r in rep)
            {
                Document doc = ConvertLoadEMDSignBundleWindowReplyToDocument(r);
                docs.Add(doc);
            }
            return docs;
        }
        private Document ConvertLoadEMDSignBundleWindowReplyToDocument(loadEMDSignBundleWindowReply r)
        {
            Document doc = new Document();
            doc.Name = r.Document_Name;
            doc.Num = r.Document_Num;
            doc.ID = r.EMDRegistry_ObjectID;
            doc.Type = r.EMDRegistry_ObjectName;
            doc.VersionID = r.EMDVersion_id;
            doc.Error = r.Error_Msg;
            doc.SignStatus = r.IsSigned;
            doc.VersionNumber = r.EMDVersion_VersionNum;
            return doc;
        }
        /// <summary>
        /// Загружаем сертификаты из ЕЦП
        /// </summary>
        public async Task<List<EcpCertificate>> LoadEcpCertificates()
        {
            List<loadEMDCertificateListReply> certs = await _emd.loadEMDCertificateList();
            List<EcpCertificate> ecpCerts = new List<EcpCertificate>(certs.Count);
            foreach (loadEMDCertificateListReply cert in certs)
            {
                EcpCertificate ecpCert = ConvertLoadEMDCertificateListReplyToEcpCertificate(cert);
                ecpCerts.Add(ecpCert);
            }
            return ecpCerts;
        }
        private EcpCertificate ConvertLoadEMDCertificateListReplyToEcpCertificate(loadEMDCertificateListReply cert)
        {
            EcpCertificate ecpCerts = new EcpCertificate();
            ecpCerts.ID = cert.EMDCertificate_id;
            ecpCerts.thumbprint = cert.EMDCertificate_SHA1.Replace("0x", "00").ToUpper();
            return ecpCerts;
        }
        /// <summary>
        /// Проверка возможности подписания
        /// </summary>
        public async Task CheckBeforeSign(Document doc, EcpCertificate ecpCert, string docName)
        {
            try
            {
                checkBeforeSignReply rep = await _emd.checkBeforeSign(doc.Type, doc.ID, ecpCert.ID, doc.VersionID);
                if (!rep.success)
                {
                    string err = rep.Error_Msg;
                    throw new DocumentSigningException(err);
                }
            }
            catch (DeserializeException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
        }
        /// <summary>
        /// Получаем документ и хеш для подписания
        /// </summary>
        public async Task<List<ToSign>> GetSignData(Document doc, EcpCertificate ecpCert, string docName)
        {
            getEMDVersionSignDataReply rep;
            try
            {
                rep = await _emd.getEMDVersionSignData(doc.Type, doc.ID, ecpCert.ID, doc.VersionNumber);
                if (!rep.success)
                {
                    string err = rep.Error_Msg;
                    throw new DocumentSigningException(err);
                }
                if (rep.toSign.Length == 0)
                {
                    throw new DocumentSigningException("GetSignData: toSign.Length = 0");
                }
            }
            catch (DeserializeException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
            List<ToSign> signs = new List<ToSign>();
            foreach (Tosign toSign in rep.toSign)
            {
                ToSign s = new ToSign();
                s.EMDVersion_id = toSign.EMDVersion_id;
                s.docBase64 = toSign.docBase64;
                s.hashBase64 = toSign.hashBase64;
                signs.Add(s);
            }
            return signs;
        }

        public async Task SaveSignature(Document doc, string VersionID, string hashBase64, string signature, EcpCertificate ecpCert, string docName)
        {
            saveEMDSignaturesReply rep;
            try
            {
                rep = await _emd.saveEMDSignatures(doc.Type, doc.ID, VersionID, hashBase64, signature, ecpCert.ID);
                if (!rep.success)
                {
                    string err = rep.Error_Msg;
                    throw new DocumentSigningException(err);
                }
            }
            catch (DeserializeException ex)
            {
                throw new IsNotLoggedInException(ex.Message);
            }
            return;
        }
    }
}
