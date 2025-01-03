﻿using System;
using System.Runtime.InteropServices;

namespace EcpSigner
{
    class FlashWindow
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

        public const UInt32 FLASHW_ALL = 0x3;
        public const UInt32 FLASHW_TIMERNOFG = 0xC;

        public static void Start(IntPtr hWnd)
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = FLASHW_ALL|FLASHW_TIMERNOFG;
            fInfo.uCount = UInt32.MaxValue;
            fInfo.dwTimeout = 0;

            FlashWindowEx(ref fInfo);
        }
        public const UInt32 FLASHW_STOP = 0;
        public static void Stop(IntPtr hWnd)
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = FLASHW_STOP;
            fInfo.uCount = UInt32.MaxValue;
            fInfo.dwTimeout = 0;

            FlashWindowEx(ref fInfo);
        }
    }
}
