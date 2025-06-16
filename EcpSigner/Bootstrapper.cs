using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Factories;
using System;

namespace EcpSigner
{
    public class Bootstrapper
    {
        private readonly IProgramRunnerFactory _runnerFactory;
        private readonly ILogger _logger;

        public Bootstrapper(IProgramRunnerFactory runnerFactory, ILogger logger)
        {
            _runnerFactory = runnerFactory;
            _logger = logger;
        }

        public void Run(string[] args)
        {
            var configPath = "config.json";
            try
            {
                var _runner = _runnerFactory.Create(configPath, _logger);
                _runner.RunAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Bootstrapper.Run: {ex.Message}");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}