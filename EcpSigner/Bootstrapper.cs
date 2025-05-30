using EcpSigner.Application.Interfaces;
using EcpSigner.Infrastructure.Factories;
using EcpSigner.Infrastructure.Services;
using System;

namespace EcpSigner
{
    public class Bootstrapper
    {
        public void Run(string[] args, IProgramRunner runner = null)
        {
            var configPath = "config.json";
            var logger = new NLogLogger(NLog.LogManager.GetLogger("EcpSigner"));
            try
            {
                var factory = new DefaultWorkerFactory(logger, configPath);
                runner = runner ?? new ProgramRunner(logger, factory);
                runner.RunAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Fatal($"Program.Run: {ex.Message}");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}