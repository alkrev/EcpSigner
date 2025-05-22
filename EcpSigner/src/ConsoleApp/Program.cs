using DocumentSigner.Application.Jobs;
using EcpSigner.Application.Decorators;
using EcpSigner.Application.Jobs;
using EcpSigner.Infrastructure.Configuration;
using EcpSigner.Infrastructure.Decorators;
using EcpSigner.Infrastructure.Repositories;
using EcpSigner.Infrastructure.Services;
using EcpSigner.Infrastructure.WebClients;
using EcpSigner.Infrastructure.Workers;
using NLog;
using Portal;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsTools;

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
                var webClient = new WebClient(new WebTools.Client(config.Get().url));
                var portalServiceDecorator = new PortalServiceDecorator(new PortalService(new Main(webClient), new EMD(webClient)), logger);
                var crypto = new CryptographyTools.Signing.CryptoPro.Crypto();
                var store = new CryptographyTools.Store.CurrentUserStore();
                var signatureServiceDecorator = new SignatureServiceDecorator(new SignatureService(crypto, store), logger);
                var cache = new Cache(new CacheTools.Cache(config.Get().cacheMinutes));
                var dates = new DatesService(args);
                var flashWindowService = new FlashWindowService(new FlashWindow(Process.GetCurrentProcess().MainWindowHandle));
                // Бизнес-логика (приложение)
                var signDocumentWorflow = new SignDocumentWorflow(portalServiceDecorator, signatureServiceDecorator, logger);
                var signDocumentsLoopDecorator = new SignDocumentsLoopDecorator(new SignDocumentsLoop(logger, config, signDocumentWorflow), cache, logger);
                var prepareSigningWorkflow = new PrepareSigningWorkflow(portalServiceDecorator, signatureServiceDecorator, logger, config, dates, cache, flashWindowService, signDocumentsLoopDecorator);
                var documentSigningJob = new DocumentSigningJob(prepareSigningWorkflow, logger);
                // Название программы
                var appTitleService = new AppTitleService(logger);
                // Источник остановки работы
                var ccs = new ConsoleCtrlCCancellationService();
                ccs.StartListening();
                var source = ccs.GetCancellationTokenSource();
                // Запуск
                var worker = new DocumentSigningWorker(documentSigningJob, logger, config, appTitleService);
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
