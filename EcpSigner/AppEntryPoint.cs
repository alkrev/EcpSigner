using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Services;
using System;

namespace EcpSigner
{
    public class AppEntryPoint
    {
        private readonly IBootstrapper _bootstrapper;
        private readonly ILoggerFactory _loggerFactory;

        public AppEntryPoint(IBootstrapper bootstrapper, ILoggerFactory loggerFactory)
        {
            _bootstrapper = bootstrapper;
            _loggerFactory = loggerFactory;
        }

        public void Run(string[] args)
        {
            var logger = _loggerFactory.Create("EcpSigner");
            try
            {
                _bootstrapper.Run(args, logger);
            }
            catch (Exception ex)
            {
                logger.Fatal($"AppEntryPoint: {ex.Message}");
            }
            finally
            {
                logger.Shutdown();
            }
        }
    }
}
