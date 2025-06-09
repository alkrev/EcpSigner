using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;

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
