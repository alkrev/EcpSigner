using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Services
{
    public class ConsoleCtrlCCancellationService: IConsoleControlService
    {
        /** 
        * Обрабатываем Ctrl-C
        */
        private void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cancelTokenSource.Cancel();
        }

        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private bool _disposed;
        public void StartListening()
        {
            Console.CancelKeyPress += CancelKeyPressHandler;
        }

        public CancellationTokenSource GetCancellationTokenSource() => cancelTokenSource;
        public void Dispose()
        {
            if (_disposed) return;
            Console.CancelKeyPress -= CancelKeyPressHandler;
            _disposed = true;
        }
    }
}
