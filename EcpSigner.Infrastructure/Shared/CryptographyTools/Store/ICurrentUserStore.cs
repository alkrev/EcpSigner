using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptographyTools.Store
{
    public interface ICurrentUserStore
    {
        Dictionary<string, CAPICOM.ICertificate> GetUserCertificates();
    }
}
