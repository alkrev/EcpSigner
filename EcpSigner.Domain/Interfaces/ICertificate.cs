using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Interfaces
{
    public interface ICertificate
    {
        string Subject { get; }
        DateTime ValidToDate { get; }
    }
}
