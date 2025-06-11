using EcpSigner.Infrastructure.Factories;

namespace EcpSigner
{
    public class Program
    {
        /// <summary>
        /// Точка входа
        /// </summary>
        public static void Main(string[] args)
        {
            var runnerFactory = new ProgramRunnerFactory();
            new Bootstrapper(runnerFactory).Run(args);
        }
    }
}
