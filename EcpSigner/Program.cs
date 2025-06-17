using EcpSigner.Infrastructure.Factories;
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
            try
            {
                var runnerFactory = new ProgramRunnerFactory();
                var bootstrapper = new Bootstrapper(runnerFactory);
                var loggerFactory = new LoggerFactory();
                new AppEntryPoint(bootstrapper, loggerFactory).Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Main: {ex.Message}");
            }
        }
    }
}
