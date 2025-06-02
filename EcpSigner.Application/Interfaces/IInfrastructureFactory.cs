using EcpSigner.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Interfaces
{
    public interface IInfrastructureFactory
    {
        IPortalService CreatePortalService();
        ISignatureService CreateSignatureService();
        ICacheService CreateCacheService();
        IDatesService CreateDatesService(string[] args);
        IFlashWindowService CreateFlashWindowService();
        IDateTimeProvider CreateDateTimeProvider();
        IDelayProvider CreateDelayProvider();
        IConfigurationProvider CreateConfigurationProvider();
    }
}
