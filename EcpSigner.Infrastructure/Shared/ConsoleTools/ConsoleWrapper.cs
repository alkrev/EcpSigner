using System;

namespace ConsoleTools
{
    public class ConsoleWrapper : IConsoleWrapper
    {
        public event ConsoleCancelEventHandler CancelKeyPress
        {
            add => Console.CancelKeyPress += value;
            remove => Console.CancelKeyPress -= value;
        }
    }
}
