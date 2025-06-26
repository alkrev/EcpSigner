using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Models
{
    public class AppSettings
    {
        /// <summary>
        /// Логин пользователя с правами админстратора МО
        /// </summary>
        public string login { get; set; }
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string password { get; set; }
        /// <summary>
        /// адрес сайта ЕЦП
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// Пауза после завершения подписания документов до следующего поиска
        /// </summary>
        public int pauseMinutes { get; set; }
        /// <summary>
        /// Время кеширования документов, при подписании которых были ошибки
        /// </summary>
        public int cacheMinutes { get; set; }
        /// <summary>
        /// Дополнительный интервал между подписанием документов
        /// </summary>
        public int signingIntervalSeconds { get; set; }
        /// <summary>
        /// Список типов документов, которые будут игнорироваться
        /// </summary>
        public Dictionary<string, byte> ignoreDocTypesDict { get; set; }
        /// <summary>
        /// UserAgent браузера, для совместимости с ЕЦП
        /// </summary>
        public string userAgent { get; set; }
    }
}
