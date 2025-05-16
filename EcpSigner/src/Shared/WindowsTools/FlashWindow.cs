using System;
using System.Runtime.InteropServices;

namespace WindowsTools
{
    class FlashWindow : IFlashWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }
        private readonly IntPtr _hWnd;
        public FlashWindow(IntPtr hWnd)
        {
            _hWnd = hWnd;
        }
        public const UInt32 FLASHW_ALL = 0x3;
        public const UInt32 FLASHW_TIMERNOFG = 0xC;
        public void Start()
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = _hWnd;
            fInfo.dwFlags = FLASHW_ALL|FLASHW_TIMERNOFG;
            fInfo.uCount = UInt32.MaxValue;
            fInfo.dwTimeout = 0;

            FlashWindowEx(ref fInfo);
        }
        public const UInt32 FLASHW_STOP = 0;

        public void Stop()
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = _hWnd;
            fInfo.dwFlags = FLASHW_STOP;
            fInfo.uCount = UInt32.MaxValue;
            fInfo.dwTimeout = 0;

            FlashWindowEx(ref fInfo);
        }
    }
}
