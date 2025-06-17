using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Factories
{
    public class ProgramRunner : IProgramRunner
    {
        private readonly ILogger _logger;
        private readonly IWorkerFactory _workerFactory;
        private readonly ICancellationService _cancellationService;

        public ProgramRunner(ILogger logger, IWorkerFactory workerFactory, ICancellationService cancellationService)
        {
            _logger = logger;
            _workerFactory = workerFactory;
            _cancellationService = cancellationService;
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
                _logger.Fatal($"ProgramRunner.RunAsync: {ex.Message}");
            }
        }
    }
}
