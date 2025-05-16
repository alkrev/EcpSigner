using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure
{
    public class NLogLogger: ILogger
    {
        private readonly NLog.Logger _logger;

        public NLogLogger(NLog.Logger logger)
        {
            _logger = logger;
        }
        public void Debug(string message) => _logger.Debug(message);
        public void Info(string message) => _logger.Info(message);
        public void Error(string message) => _logger.Error(message);
        public void Warn(string message) => _logger.Warn(message);
        public void Fatal(string message) => _logger.Fatal(message);
    }
}
