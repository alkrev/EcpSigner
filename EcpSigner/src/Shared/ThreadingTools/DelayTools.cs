using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingTools
{
    public static class DelayTools
    {
        /// <summary>
        /// Подавляем генерацию исключения
        /// </summary>
        public static async Task Delay(TimeSpan ts, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(ts, cancellationToken);
            }
            catch (TaskCanceledException) { }
        }
    }
}
