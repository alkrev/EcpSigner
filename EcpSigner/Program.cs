using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Services;
using System;

namespace EcpSigner
{
    public class Program
    {
        /// <summary>
        /// Точка входа
        /// </summary>
        public static void Main(string[] args)
        {
            var logger = new NLogLogger(NLog.LogManager.GetLogger("EcpSigner"));
            try
            {
                var runnerFactory = new ProgramRunnerFactory();
                new Bootstrapper(runnerFactory, logger).Run(args);
            }
            catch (Exception ex)
            {
                logger.Fatal($"Main: {ex.Message}");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}
