using EcpSigner.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using System.Threading;
using EcpSigner.Application.Interfaces;

namespace EcpSigner
{
    public class ProgramRunner
    {
        private readonly ILogger _logger;
        private readonly IWorkerFactory _workerFactory;

        public ProgramRunner(ILogger logger, IWorkerFactory workerFactory)
        {
            _logger = logger;
            _workerFactory = workerFactory;
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
                var _worker = _workerFactory.CreateWorker(args);
                await _worker.RunAsync(cts.Token);
            }
            catch (Exception ex)
            {
                _logger.Fatal($"RunAsync: {ex.Message}");
            }
        }
    }
}
