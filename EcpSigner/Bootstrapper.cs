using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Services;
using System;

namespace EcpSigner
{
    public class Bootstrapper
    {
        private readonly IProgramRunnerFactory _runnerFactory;

        public Bootstrapper(IProgramRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        public void Run(string[] args)
        {
            var configPath = "config.json";
            var logger = new NLogLogger(NLog.LogManager.GetLogger("EcpSigner"));
            try
            {
                var _runner = _runnerFactory.Create(configPath, logger);
                _runner.RunAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Fatal($"Bootstrapper.Run: {ex.Message}");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}