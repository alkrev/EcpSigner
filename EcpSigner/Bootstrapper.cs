using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Services;
using System;

namespace EcpSigner
{
    public class Bootstrapper
    {
        public void Run(string[] args, IProgramRunner runner = null)
        {
            var configPath = "config.json";
            var logger = new NLogLogger(NLog.LogManager.GetLogger("EcpSigner"));
            try
            {
                var configurationFactory = new ConfigurationProviderFactory(logger, configPath);
                var webClientFactory = new WebClientFactory();
                var cryptoFactory = new CryptoFactory();
                var storeFactory = new StoreFactory();
                var cacheFactory = new CacheFactory();
                var flashWindowFactory = new FlashWindowFactory();
                var dateTimeProviderFactory = new DateTimeProviderFactory();
                var infrastructureFactory = new InfrastructureFactory(
                    logger, 
                    configurationFactory,
                    webClientFactory,
                    cryptoFactory,
                    storeFactory,
                    cacheFactory,
                    flashWindowFactory,
                    dateTimeProviderFactory
                    );
                var workerFactory = new DefaultWorkerFactory(logger, infrastructureFactory);
                runner = runner ?? new ProgramRunner(logger, workerFactory);
                runner.RunAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Fatal($"Program.Run: {ex.Message}");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}