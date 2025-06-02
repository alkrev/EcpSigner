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
        private readonly IInfrastructureFactory _infraFactory;
        public DefaultWorkerFactory(ILogger logger, IInfrastructureFactory infraFactory)
        {
            _logger = logger;
            _infraFactory = infraFactory;
        }

        public IJob CreateWorker(string[] args)
        {
            // Инфраструктура с использованием библиотек
            var config = _infraFactory.CreateConfigurationProvider();
            var portalService = _infraFactory.CreatePortalService();
            var signatureService = _infraFactory.CreateSignatureService();
            var cache = _infraFactory.CreateCacheService();
            var dates = _infraFactory.CreateDatesService(args);
            var flashWindowService = _infraFactory.CreateFlashWindowService();
            var delayProvider = _infraFactory.CreateDelayProvider();
            var dateTimeProvider = _infraFactory.CreateDateTimeProvider();
            // Бизнес-логика (приложение)
            var signDocumentWorflow = new SignDocumentWorflow(portalService, signatureService, _logger);
            var signDocumentsLoopDecorator = new SignDocumentsLoopDecorator(
                new SignDocumentsLoop(_logger, config, signDocumentWorflow, delayProvider),
                cache,
                _logger,
                delayProvider
            );
            var prepareSigningWorkflow = new PrepareSigningWorkflow(
                portalService,
                signatureService,
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
