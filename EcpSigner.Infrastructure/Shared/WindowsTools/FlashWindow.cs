using System;
using System.Runtime.InteropServices;

namespace WindowsTools
{
    public class FlashWindow : IFlashWindow
    {
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }

        private readonly IntPtr _hWnd;
        private readonly IFlashWindowNative _native;

        public const UInt32 FLASHW_ALL = 0x3;
        public const UInt32 FLASHW_TIMERNOFG = 0xC;
        public const UInt32 FLASHW_STOP = 0;

        public FlashWindow(IntPtr hWnd, IFlashWindowNative native)
        {
            _hWnd = hWnd;
            _native = native;
        }

        public void Start()
        {
            FLASHWINFO fInfo = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf<FLASHWINFO>()),
                hwnd = _hWnd,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = UInt32.MaxValue,
                dwTimeout = 0
            };
            _native.Flash(ref fInfo);
        }

        public void Stop()
        {
            FLASHWINFO fInfo = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf<FLASHWINFO>()),
                hwnd = _hWnd,
                dwFlags = FLASHW_STOP,
                uCount = UInt32.MaxValue,
                dwTimeout = 0
            };
            _native.Flash(ref fInfo);
        }
    }
}
