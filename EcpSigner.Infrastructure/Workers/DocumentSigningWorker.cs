using System;
using System.Threading;
using System.Threading.Tasks;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Application.Interfaces;

namespace EcpSigner.Infrastructure.Workers
{
    public class DocumentSigningWorker: IJob
    {
        private readonly IJob _job;
        private readonly ILogger _logger;
        private readonly IConfigurationProvider _config;
        private readonly IAppTitleService _appTitleService;
        private readonly IDelayProvider _delayProvider;
        public DocumentSigningWorker(IJob job, ILogger logger, IConfigurationProvider config, IAppTitleService appTitleService, IDelayProvider delayProvider)
        {
            _job = job;
            _logger = logger;
            _config = config;
            _appTitleService = appTitleService;
            _delayProvider = delayProvider;
        }
        /// <summary>
        /// Основной цикл работы
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                _appTitleService.Set();
                while (!cancellationToken.IsCancellationRequested)
                {
                    await _job.RunAsync(cancellationToken);
                    await _delayProvider.DelayAsync(TimeSpan.FromMinutes(_config.Get().pauseMinutes), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"остановка: {ex.Message}");
            }
            _logger.Info("работа завершена");
        }
    }
}
