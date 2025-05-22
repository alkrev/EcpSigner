using System;
using System.Threading;
using System.Threading.Tasks;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Application.Interfaces;

namespace EcpSigner.Infrastructure.Workers
{
    public class DocumentSigningWorker
    {
        private readonly IJob _job;
        private readonly ILogger _logger;
        private readonly IConfigurationProvider _config;
        private readonly IAppTitleService _appTitleService;
        public DocumentSigningWorker(IJob job, ILogger logger, IConfigurationProvider config, IAppTitleService appTitleService)
        {
            _job = job;
            _logger = logger;
            _config = config;
            _appTitleService = appTitleService;
        }
        /// <summary>
        /// Основной цикл работы
        /// </summary>
        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                _appTitleService.Set();
                while (!cancellationToken.IsCancellationRequested)
                {
                    await _job.RunAsync(cancellationToken);
                    await Application.Tools.DelayTools.Delay(TimeSpan.FromMinutes(_config.Get().pauseMinutes), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"остановка: {ex.Message ?? "необработанное исключение"}");
            }
            _logger.Info("работа завершена");
        }
    }
}
