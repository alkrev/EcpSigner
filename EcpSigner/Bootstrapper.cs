using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Factories;
using System;

namespace EcpSigner
{
    public class Bootstrapper: IBootstrapper
    {
        private readonly IProgramRunnerFactory _runnerFactory;

        public Bootstrapper(IProgramRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        public void Run(string[] args, ILogger logger)
        {
            var configPath = "config.json";
            try
            {
                var _runner = _runnerFactory.Create(configPath, logger);
                _runner.RunAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Fatal($"Bootstrapper.Run: {ex.Message}");
            }
        }
    }
}