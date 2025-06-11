using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public interface IProgramRunnerFactory
    {
        IProgramRunner Create(string configPath, ILogger logger);
    }
}
