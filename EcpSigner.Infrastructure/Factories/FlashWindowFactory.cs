using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsTools;

namespace EcpSigner.Infrastructure.Factories
{
    public class FlashWindowFactory : IFlashWindowFactory
    {
        public IFlashWindow Create()
        {
            return new FlashWindow(Process.GetCurrentProcess().MainWindowHandle);
        }
    }
}
