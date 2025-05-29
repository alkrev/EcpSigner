using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Services
{
    public class ConsoleCancellationService : ICancellationService
    {
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ConsoleCancellationService(ILogger logger)
        {
            _logger = logger;
        }

        public CancellationToken Token => _cts.Token;

        public void StartListeningForCancel()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _logger.Info("Ctrl-C нажато");
                _cts.Cancel();
            };
        }
    }
}
