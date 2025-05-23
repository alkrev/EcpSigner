using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Models
{
    public class EcpCertificate
    {
        ///<summary>ID сертификата в ЕЦП</summary>
        public string ID;
        ///<summary>SHA-1 отпечаток</summary>
        public string thumbprint;
    }
}
