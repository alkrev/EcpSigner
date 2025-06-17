using CachingTools;
using EcpSigner.Domain.Interfaces;

namespace EcpSigner.Infrastructure.Factories
{
    public interface ICacheFactory
    {
        ICache Create(int minutes, IDateTimeProvider dateTimeProvider);
    }
}
