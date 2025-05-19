using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure
{
    public class ConsoleControlService : IConsoleControlService, IDisposable
    {
        private const int CTRL_C_EVENT = 0;
        private const int CTRL_CLOSE_EVENT = 2;
        private const int CTRL_LOGOFF_EVENT = 5;
        private const int CTRL_SHUTDOWN_EVENT = 6;
        private const int SW_HIDE = 0;
        private const int CW_USEDEFAULT = unchecked((int)0x80000000);

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _disposed;
        private IntPtr _windowHandle;
        /// <summary>
        /// Служба для корректного завершения работы
        /// </summary>
        public ConsoleControlService()
        {
            ConsoleCtrlHandler = ConsoleCtrlHandlerProc;
        }

        public CancellationTokenSource GetCancellationTokenSource() => _cts;

        public void StartListening()
        {
            SetConsoleCtrlHandler(ConsoleCtrlHandler, true);
            StartMessageLoop();
        }

        private void StartMessageLoop()
        {
            new Thread(() =>
            {
                InitializeWindow();
                MessageLoop();
            })
            { IsBackground = true }.Start();
        }

        #region Native Interop

        private delegate bool ConsoleCtrlHandlerDelegate(int dwCtrlType);
        private readonly ConsoleCtrlHandlerDelegate ConsoleCtrlHandler;

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handler, bool add);

        private bool ConsoleCtrlHandlerProc(int dwCtrlType)
        {
            switch (dwCtrlType)
            {
                case CTRL_C_EVENT:
                case CTRL_CLOSE_EVENT:
                case CTRL_LOGOFF_EVENT:
                case CTRL_SHUTDOWN_EVENT:
                    _cts.Cancel();
                    return true;
                default:
                    return false;
            }
        }

        #region Window Handling

        private const uint WM_QUERYENDSESSION = 0x0011;
        private const uint WM_ENDSESSION = 0x0016;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASSEX
        {
            public int cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
            public uint lPrivate;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        private void InitializeWindow()
        {
            var windowClass = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
                lpszClassName = "CtrlHandlerClass",
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(
                    new WndProcDelegate(WindowProc))
            };

            if (RegisterClassEx(ref windowClass) == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            _windowHandle = CreateWindowEx(
                0,
                "CtrlHandlerClass",
                "Console Control Handler hidden window",
                0,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            if (_windowHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            ShowWindow(_windowHandle, SW_HIDE);
        }

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        private IntPtr WindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            switch (uMsg)
            {
                case WM_QUERYENDSESSION:
                    return (IntPtr)1;
                case WM_ENDSESSION:
                    if (wParam != IntPtr.Zero)
                        _cts.Cancel();
                    return IntPtr.Zero;
                default:
                    return DefWindowProc(hWnd, uMsg, wParam, lParam);
            }
        }

        private void MessageLoop()
        {
            while (true)
            {
                if (!GetMessage(out var msg, IntPtr.Zero, 0, 0))
                    break;

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        #endregion

        #endregion

        public void Dispose()
        {
            if (_disposed) return;

            _cts.Dispose();
            if (_windowHandle != IntPtr.Zero)
            {
                DestroyWindow(_windowHandle);
                _windowHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }
}
