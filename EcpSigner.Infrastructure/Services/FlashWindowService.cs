using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsTools;

namespace EcpSigner.Infrastructure.Services
{
    public class FlashWindowService : IFlashWindowService
    {
        private bool _isFlashing;
        private readonly IFlashWindow _flashWindow;
        public FlashWindowService(IFlashWindow flashWindow)
        {
            _flashWindow = flashWindow;
        }
        public void Start()
        {
            _flashWindow.Start();
            _isFlashing = true;
        }
        public void Stop()
        {
            _flashWindow.Stop();
            _isFlashing = false;
        }
        public bool IsFlashing() => _isFlashing;
    }
}
