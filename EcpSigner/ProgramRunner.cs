using CachingTools;
using Ecp.Portal;
using Ecp.Web;
using EcpSigner.Application.Decorators;
using EcpSigner.Application.Jobs;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Configuration;
using EcpSigner.Infrastructure.Decorators;
using EcpSigner.Infrastructure.Repositories;
using EcpSigner.Infrastructure.Services;
using EcpSigner.Infrastructure.WebClients;
using EcpSigner.Infrastructure.Workers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsTools;
using CryptographyTools.Signing.CryptoPro;
using CryptographyTools.Store;
using System.Threading;

namespace EcpSigner
{
    public class ProgramRunner
    {
        private readonly ILogger _logger;

        public ProgramRunner(ILogger logger)
        {
            _logger = logger;
        }

        public async Task RunAsync(string[] args)
        {
            var cts = new CancellationTokenSource();

            // Обработка Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _logger.Info("Ctrl+C нажато. Остановка работы");
                cts.Cancel();
            };

            try
            {
                // Инфраструктура с использованием библиотек
                var config = new JsonConfigurationProvider(_logger, "config.json");
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
                // Запуск
                var worker = new DocumentSigningWorker(documentSigningJob, _logger, config, appTitleService, delayProvider);
                await worker.Run(cts.Token);
            }
            catch (Exception ex)
            {
                _logger.Fatal($"RunAsync: {ex.Message ?? "фатальная ошибка"}");
            }
        }
    }
}
