using DocumentSigner.Application.Jobs;
using EcpSigner.Application.Interfaces;
using EcpSigner.Application.Jobs;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using EcpSigner.Infrastructure;
using EcpSigner.Infrastructure.Configuration;
using EcpSigner.Infrastructure.Decorators;
using EcpSigner.Infrastructure.Repositories;
using EcpSigner.Infrastructure.WebClients;
using EcpSigner.Infrastructure.Workers;
using Portal;
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
            // Собираем DI
            // Инфраструктура с использованием библиотек
            var logger = new NLogLogger(NLog.LogManager.GetLogger("EcpSigner"));
            var config = new JsonConfigurationProvider(logger, "config.json");
            var webClient = new WebClient(new WebTools.Client(config.Get().url));
            var portalServiceDecorator = new PortalServiceDecorator(new PortalService(new Main(webClient), new EMD(webClient) ), logger);
            var crypto = new CryptographyTools.Signing.CryptoPro.Crypto();
            var store = new CryptographyTools.Store.CurrentUserStore();
            var signatureServiceDecorator = new SignatureServiceDecorator(new SignatureService(crypto, store), logger);
            var cache = new Cache(new CacheTools.Cache(config.Get().cacheMinutes));
            var dates = new DatesService(args);
            var flashWindowService = new FlashWindowService(new FlashWindow(Process.GetCurrentProcess().MainWindowHandle));
            // Бизнес-логика (приложение)
            var signDocumentWorflow = new SignDocumentWorflow(portalServiceDecorator, signatureServiceDecorator, logger);
            var signDocumentsLoop = new SignDocumentsLoop(logger, config, signDocumentWorflow);
            var prepareSigningWorkflow = new PrepareSigningWorkflow(portalServiceDecorator, signatureServiceDecorator, logger, config, dates, cache, flashWindowService, signDocumentsLoop);
            var documentSigningJob = new DocumentSigningJob(prepareSigningWorkflow, logger);
            // Название программы
            new AppTitleService(logger).Set();
            // Источник остановки работы
            var source = new ConsoleCtrlCCancellationTokenSource();
            // Запуск
            var worker = new DocumentSigningWorker(documentSigningJob, logger, config);
            Task.Run(async () => await worker.Run(source.Token)).Wait();
        }
    }
}
