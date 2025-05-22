using EcpSigner.Domain.Interfaces;
using System;

namespace EcpSigner.Infrastructure
{
    public class AppTitleService : IAppTitleService
    {
        private readonly ILogger _logger;
        public AppTitleService(ILogger logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Установка названия программы
        /// </summary>
        public void Set()
        {
            string title = GetAppTitle();
            _logger.Info(title);
            Console.Title = title;
        }
        /// <summary>
        /// Название программы с версией
        /// </summary>
        public string GetAppTitle()
        {
            string name = System.Reflection.Assembly.GetEntryAssembly().GetName().Name.ToString();
            string ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            return $"{name} v{ver}";
        }
    }
}
