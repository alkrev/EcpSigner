using CachingTools;
using CryptographyTools.Signing.CryptoPro;
using CryptographyTools.Store;
using Ecp.Portal;
using Ecp.Web;
using EcpSigner.Application.Decorators;
using EcpSigner.Application.Interfaces;
using EcpSigner.Application.Jobs;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Configuration;
using EcpSigner.Infrastructure.Decorators;
using EcpSigner.Infrastructure.Repositories;
using EcpSigner.Infrastructure.Services;
using EcpSigner.Infrastructure.WebClients;
using EcpSigner.Infrastructure.Workers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsTools;

namespace EcpSigner.Infrastructure.Factories
{
    public class DefaultWorkerFactory: IWorkerFactory
    {
        private readonly ILogger _logger;
        private readonly string _configPath;
        public DefaultWorkerFactory(ILogger logger, string configPath)
        {
            _logger = logger;
            _configPath = configPath;
        }

        public IJob CreateWorker(string[] args)
        {
            // Инфраструктура с использованием библиотек
            var config = new JsonConfigurationProvider(_logger, _configPath);
            var webClient = new WebClient(new Client(config.Get().url));
            var portalServiceDecorator = new PortalServiceDecorator(new PortalService(new Main(webClient), new EMD(webClient)), _logger);
            var crypto = new Crypto();
            var store = new CurrentUserStore();
            var signatureServiceDecorator = new SignatureServiceDecorator(new SignatureService(crypto, store), _logger);
            var cache = new CacheService(new Cache(config.Get().cacheMinutes));
            var dates = new DatesService(args);
            var flashWindowService = new FlashWindowService(new FlashWindow(Process.GetCurrentProcess().MainWindowHandle));
            var delayProvider = new DelayProvider();
            var dateTimeProvider = new DateTimeProvider();
            // Бизнес-логика (приложение)
            var signDocumentWorflow = new SignDocumentWorflow(portalServiceDecorator, signatureServiceDecorator, _logger);
            var signDocumentsLoopDecorator = new SignDocumentsLoopDecorator(
                new SignDocumentsLoop(_logger, config, signDocumentWorflow, delayProvider),
                cache,
                _logger,
                delayProvider
            );
            var prepareSigningWorkflow = new PrepareSigningWorkflow(
                portalServiceDecorator,
                signatureServiceDecorator,
                _logger,
                config,
                dates,
                cache,
                flashWindowService,
                signDocumentsLoopDecorator,
                dateTimeProvider
            );
            var documentSigningJob = new DocumentSigningJob(prepareSigningWorkflow, _logger);
            // Название программы
            var appTitleService = new AppTitleService(_logger);
            return new DocumentSigningWorker(documentSigningJob, _logger, config, appTitleService, delayProvider);
        }
    }
}
