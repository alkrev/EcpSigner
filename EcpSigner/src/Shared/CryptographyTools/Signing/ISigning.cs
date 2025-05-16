using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptographyTools.Signing
{
    public interface ISigning
    {
        string Sign(CAPICOM.ICertificate certificate, string docBase64);
    }
}
