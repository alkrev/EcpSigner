using System.Diagnostics;
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
