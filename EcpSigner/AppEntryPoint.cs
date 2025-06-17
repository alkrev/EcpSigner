using EcpSigner.Infrastructure.Services;
using System;

namespace EcpSigner
{
    public class AppEntryPoint
    {
        private readonly IBootstrapper _bootstrapper;

        public AppEntryPoint(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public void Run(string[] args)
        {
            var logger = new NLogLogger(NLog.LogManager.GetLogger("EcpSigner"));
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
                NLog.LogManager.Shutdown();
            }
        }
    }
}
