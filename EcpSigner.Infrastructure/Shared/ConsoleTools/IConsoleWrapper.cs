using System;

namespace ConsoleTools
{
    public interface IConsoleWrapper
    {
        event ConsoleCancelEventHandler CancelKeyPress;
    }
}
