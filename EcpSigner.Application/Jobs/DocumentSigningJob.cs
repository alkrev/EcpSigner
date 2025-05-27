using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EcpSigner.Application.Jobs
{
    public class DocumentSigningJob : IJob
    {
        private readonly IJob _prepareSigningWorkflow;
        private readonly ILogger _logger;

        public DocumentSigningJob(
            IJob workflow, 
            ILogger logger
        )
        {
            _prepareSigningWorkflow = workflow;
            _logger = logger;
        }
        /// <summary>
        /// Подписываем документы
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _prepareSigningWorkflow.RunAsync(cancellationToken);
            }
            catch (BreakWorkException ex)
            {
                string m = $"DocumentSigningJob: {ex.Message ?? "фатальная ошибка"}";
                _logger.Fatal(m);
                throw new Exception(m);
            }
            catch (StopWorkException)
            {
                string m = "остановка работы";
                _logger.Info(m);
            }
            catch (IsNotLoggedInException)
            {
                _logger.Warn("вход не выполнен");
            }
            catch (ContinueException ex)
            {
                _logger.Debug(ex.Message);
            }
            catch (ContinueExceptionWithError ex)
            {
                _logger.Error($"DocumentSigningJob: {ex.Message ?? "ошибка"}");
            }
            catch (Exception ex)
            {
                _logger.Error($"DocumentSigningJob: {ex.Message ?? "необработанная ошибка"}");
                _logger.Debug($"DocumentSigningJob: {ex.GetType().Name}");
                _logger.Debug($"DocumentSigningJob: {ex.StackTrace}");
            }
        }
    }
}