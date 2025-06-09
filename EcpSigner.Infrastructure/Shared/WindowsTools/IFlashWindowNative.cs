using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsTools;

namespace WindowsTools
{
    public interface IFlashWindowNative
    {
        bool Flash(ref FlashWindow.FLASHWINFO fInfo);
    }
}
