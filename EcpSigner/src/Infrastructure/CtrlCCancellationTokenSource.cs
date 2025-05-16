using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure
{
    class ConsoleCtrlCCancellationTokenSource
    {
        /** 
        * Обрабатываем Ctrl-C
        */
        CancellationTokenSource cancelTokenSource;
        private void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cancelTokenSource.Cancel();
        }
        private CancellationToken token;

        public CancellationToken Token { get => token; }

        public ConsoleCtrlCCancellationTokenSource()
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            Console.CancelKeyPress += CancelKeyPressHandler;
        }
    }
}
