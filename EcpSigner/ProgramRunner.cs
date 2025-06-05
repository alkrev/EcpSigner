using ConsoleTools;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace EcpSigner
{
    public class ProgramRunner: IProgramRunner
    {
        private readonly ILogger _logger;
        private readonly IWorkerFactory _workerFactory;
        private readonly ICancellationService _cancellationService;

        public ProgramRunner(ILogger logger, IWorkerFactory workerFactory, ICancellationService cancellationService = null)
        {
            _logger = logger;
            _workerFactory = workerFactory;
            _cancellationService = cancellationService ?? new ConsoleCancellationService(logger, new ConsoleWrapper());
        }

        public async Task RunAsync(string[] args)
        {
            _cancellationService.StartListeningForCancel();
            var token = _cancellationService.Token;
            try
            {
                var _worker = _workerFactory.CreateWorker(args);
                await _worker.RunAsync(token);
            }
            catch (Exception ex)
            {
                _logger.Fatal($"RunAsync: {ex.Message}");
            }
        }
    }
}
