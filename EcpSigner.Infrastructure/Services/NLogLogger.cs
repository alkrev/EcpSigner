using EcpSigner.Domain.Interfaces;

namespace EcpSigner.Infrastructure.Services
{
    public class NLogLogger: ILogger
    {
        private readonly NLog.ILogger _logger;

        public NLogLogger(NLog.ILogger logger)
        {
            _logger = logger;
        }
        public void Flush() => NLog.LogManager.Flush();
        public void Debug(string message) => _logger.Debug(message);
        public void Info(string message) => _logger.Info(message);
        public void Error(string message) => _logger.Error(message);
        public void Warn(string message) => _logger.Warn(message);
        public void Fatal(string message) => _logger.Fatal(message);
    }
}
