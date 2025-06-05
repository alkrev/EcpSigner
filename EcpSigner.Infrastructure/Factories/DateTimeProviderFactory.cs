using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public class DateTimeProviderFactory : IDateTimeProviderFactory
    {
        public IDateTimeProvider Create()
        {
            return new DateTimeProvider();
        }
    }
}
