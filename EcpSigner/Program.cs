using EcpSigner.Infrastructure.Services;
using NLog;
using System;
using System.Threading.Tasks;
namespace EcpSigner
{
    class Program
    {
        /// <summary>
        /// Точка входа
        /// </summary>
        static void Main(string[] args)
        {
            var logger = new NLogLogger(LogManager.GetLogger("EcpSigner"));
            try
            {
                var runner = new ProgramRunner(logger);
                Task.Run(() => runner.RunAsync(args)).Wait();
            }
            catch (Exception ex)
            {
                logger.Fatal($"Main: {ex.Message}");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}
