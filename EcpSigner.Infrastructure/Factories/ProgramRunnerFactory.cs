using ConsoleTools;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public class ProgramRunnerFactory: IProgramRunnerFactory
    {
        public IProgramRunner Create(string configPath, ILogger logger)
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
            var cancellationService = new ConsoleCancellationService(logger, new ConsoleWrapper());
            return new ProgramRunner(logger, workerFactory, cancellationService);
        }
    }
}
