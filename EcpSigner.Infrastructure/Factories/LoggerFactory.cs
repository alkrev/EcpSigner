using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;

namespace EcpSigner.Infrastructure.Factories
{
    public class LoggerFactory : ILoggerFactory
    {
        public ILogger Create(string name)
        {
            var nlogLogger = NLog.LogManager.GetLogger(name);
            return new NLogLogger(nlogLogger);
        }
    }
}
