using System.Diagnostics;
using WindowsTools;

namespace EcpSigner.Infrastructure.Factories
{
    public class FlashWindowFactory : IFlashWindowFactory
    {
        public IFlashWindow Create()
        {
            var native = new FlashWindowNative();
            return new FlashWindow(Process.GetCurrentProcess().MainWindowHandle, native);
        }
    }
}
