using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Interfaces
{
    public interface ILoggerFactory
    {
        ILogger Create(string name);
    }
}
