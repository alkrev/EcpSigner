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
        public DocumentSigningWorker(IJob job, ILogger logger, IConfigurationProvider config)
        {
            _job = job;
            _logger = logger;
            _config = config;
        }
        /// <summary>
        /// Основной цикл работы
        /// </summary>
        public async Task Run(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _job.RunAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(_config.Get().pauseMinutes), stoppingToken); // интервал между задачами
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
