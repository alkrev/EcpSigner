using ConsoleTools;
using EcpSigner.Domain.Interfaces;
using System;
using System.Threading;

namespace EcpSigner.Infrastructure.Services
{
    public class ConsoleCancellationService : ICancellationService
    {
        private readonly ILogger _logger;
        private readonly IConsoleWrapper _console;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ConsoleCancellationService(ILogger logger, IConsoleWrapper console)
        {
            _logger = logger;
            _console = console;
        }

        public CancellationToken Token => _cts.Token;

        public void StartListeningForCancel()
        {
            _console.CancelKeyPress += HandleCancelKeyPress;
        }
        internal void HandleCancelKeyPress(object sender, EventArgs e)
        {
            if (e is ConsoleCancelEventArgs cancelArgs)
            {
                cancelArgs.Cancel = true;
                _logger.Info("Ctrl-C нажато");
                _cts.Cancel();
            }
        }
    }
}
