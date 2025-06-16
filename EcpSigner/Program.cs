using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Services;

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
            var runnerFactory = new ProgramRunnerFactory();
            new Bootstrapper(runnerFactory, logger).Run(args);
        }
    }
}
