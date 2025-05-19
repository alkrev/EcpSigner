using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Portal
{
    public interface IEMD
    {
        Task<List<loadEMDSignBundleWindowReply>> loadEMDSignBundleWindow(string startDate, string endDate, int startIndex, int page, int limit, CancellationToken cancellationToken);
        Task<List<loadEMDCertificateListReply>> loadEMDCertificateList(CancellationToken cancellationToken);
        Task<checkBeforeSignReply> checkBeforeSign(string EMDRegistry_ObjectName, string EMDRegistry_ObjectID, string EMDCertificate_id, string EMDVersion_id, CancellationToken cancellationToken);
        Task<getEMDVersionSignDataReply> getEMDVersionSignData(string EMDRegistry_ObjectName, string EMDRegistry_ObjectID, string EMDCertificate_id, int EMDVersion_VersionNum, CancellationToken cancellationToken);
        Task<saveEMDSignaturesReply> saveEMDSignatures(string EMDRegistry_ObjectName, string EMDRegistry_ObjectID, string EMDVersion_id, string Signatures_Hash, string Signatures_SignedData, string EMDCertificate_id, CancellationToken cancellationToken);
    }
}
