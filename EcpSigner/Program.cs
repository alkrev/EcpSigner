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
using NLog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsTools;
using CryptographyTools.Signing.CryptoPro;
using CryptographyTools.Store;

namespace EcpSigner
{
    class Program
    {
        /// <summary>
        /// Точка входа
        /// </summary>
        static void Main(string[] args)
        {
            var logger = new NLogLogger(LogManager.GetLogger("EcpSigner"));
            try
            {
                // Собираем DI
                // Инфраструктура с использованием библиотек
                var config = new JsonConfigurationProvider(logger, "config.json");
                var webClient = new WebClient(new Client(config.Get().url));
                var portalServiceDecorator = new PortalServiceDecorator(new PortalService(new Main(webClient), new EMD(webClient)), logger);
                var crypto = new Crypto();
                var store = new CurrentUserStore();
                var signatureServiceDecorator = new SignatureServiceDecorator(new SignatureService(crypto, store), logger);
                var cache = new CacheService(new Cache(config.Get().cacheMinutes));
                var dates = new DatesService(args);
                var flashWindowService = new FlashWindowService(new FlashWindow(Process.GetCurrentProcess().MainWindowHandle));
                var delayProvider = new DelayProvider();
                var dateTimeProvider = new DateTimeProvider();
                // Бизнес-логика (приложение)
                var signDocumentWorflow = new SignDocumentWorflow(portalServiceDecorator, signatureServiceDecorator, logger);
                var signDocumentsLoopDecorator = new SignDocumentsLoopDecorator(new SignDocumentsLoop(logger, config, signDocumentWorflow, delayProvider), cache, logger, delayProvider);
                var prepareSigningWorkflow = new PrepareSigningWorkflow(portalServiceDecorator, signatureServiceDecorator, logger, config, dates, cache, flashWindowService, signDocumentsLoopDecorator, dateTimeProvider);
                var documentSigningJob = new DocumentSigningJob(prepareSigningWorkflow, logger);
                // Название программы
                var appTitleService = new AppTitleService(logger);
                // Источник остановки работы
                var ccs = new ConsoleCtrlCCancellationService();
                ccs.StartListening();
                var source = ccs.GetCancellationTokenSource();
                // Запуск
                var worker = new DocumentSigningWorker(documentSigningJob, logger, config, appTitleService, delayProvider);
                Task.Run(async () => await worker.Run(source.Token)).Wait();
            }
            catch (Exception ex)
            {
                logger.Fatal($"Main: {ex.Message ?? "фатальная ошибка"}");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}
