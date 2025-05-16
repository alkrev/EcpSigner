using EcpSigner.Application.Filters;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Exceptions;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentSigner.Application.Jobs
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
                string m = $"{ex.Message ?? "DocumentSigningJob: фатальная ошибка"}";
                _logger.Fatal(m);
                throw new Exception(m);
            }
            catch (StopWorkException)
            {
                string m = "остановка работы";
                _logger.Info(m);
                throw new Exception(m);
            }
            catch (IsNotLoggedInException)
            {
                _logger.Warn("вход не выполнен");
            }
            catch (ContinueException ex)
            {
                _logger.Debug(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message ?? "DocumentSigningJob: необработанная ошибка"}");
            }
        }
    }
}