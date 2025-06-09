using System.Runtime.InteropServices;

namespace WindowsTools
{
    public class FlashWindowNative : IFlashWindowNative
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FlashWindow.FLASHWINFO pwfi);

        public bool Flash(ref FlashWindow.FLASHWINFO fInfo) => FlashWindowEx(ref fInfo);
    }
}
