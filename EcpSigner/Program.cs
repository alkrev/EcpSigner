using EcpSigner.Infrastructure.Factories;
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
        public static void Main(string[] args)
        {
            var configPath = "config.json";
            Run(args, configPath);
        }
        public static void Run(string[] args, string configPath)
        {
            var logger = new NLogLogger(LogManager.GetLogger("EcpSigner"));
            try
            {
                var workerFactory = new DefaultWorkerFactory(logger, configPath);
                var runner = new ProgramRunner(logger, workerFactory);
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
