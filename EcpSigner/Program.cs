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
            try
            {
                var runnerFactory = new ProgramRunnerFactory();
                var bootstrapper = new Bootstrapper(runnerFactory);
                new AppEntryPoint(bootstrapper).Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Main: {ex.Message}");
            }
        }
    }
}
