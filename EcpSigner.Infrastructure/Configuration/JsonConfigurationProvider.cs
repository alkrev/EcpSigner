using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.Configuration
{
    public class JsonConfigurationProvider : IConfigurationProvider
    {
        private readonly ILogger _logger;
        private readonly string _fileName;
        private static AppSettings _appSettings;
        public JsonConfigurationProvider(ILogger logger, string fileName)
        {
            _logger = logger;
            _fileName = fileName;
            _appSettings = null;
        }
        /// <summary>
        /// Возвращаем настройки
        /// </summary>
        public AppSettings Get()
        {
            _appSettings = _appSettings ?? GetAppSettings();
            return _appSettings;
        }
        /// <summary>
        /// Получаем настройки из файла
        /// </summary>
        private AppSettings GetAppSettings()
        {
            AppSettings appSettings;
            Settings settings = Read(_fileName);
            appSettings = ConvertSettings(settings);
            CheckSettings(appSettings);
            return appSettings;
        }
        /// <summary>
        /// Читаем Json файл с настройками
        /// </summary>
        private static Settings Read(string filename)
        {
            string str = File.ReadAllText(filename);
            Settings s = JsonConvert.DeserializeObject<Settings>(str);
            return s;
        }
        /// <summary>
        /// Конвертируем настройки
        /// </summary>
        private AppSettings ConvertSettings(Settings s)
        {
            AppSettings appSettings = new AppSettings();
            appSettings.login = s.login;
            appSettings.password = s.password;
            appSettings.url = s.url;
            appSettings.pauseMinutes = s.pauseMinutes;
            appSettings.cacheMinutes = s.cacheMinutes;
            appSettings.signingIntervalSeconds = s.signingIntervalSeconds;
            appSettings.ignoreDocTypesDict = s.ignoreDocTypes.ToDictionary(x => x, x => (byte)1);
            return appSettings;
        }
        /// <summary>
        /// Проверяем настройки
        /// </summary>
        private void CheckSettings(AppSettings s)
        {
            if (string.IsNullOrEmpty(s.login))
            {
                throw new Exception("login пользователя не задан");
            }
            if (string.IsNullOrEmpty(s.password))
            {
                throw new Exception("password пользователя не задан");
            }
            if (string.IsNullOrEmpty(s.url))
            {
                throw new Exception("url сайта ЕЦП не задан");
            }
            if (s.pauseMinutes < 1 || s.pauseMinutes > 7 * 60 * 24)
            {
                s.pauseMinutes = 15;
                _logger.Warn($"pauseMinutes задан некорректно. Установлено pauseMinutes={s.pauseMinutes}");
            }
            if (s.cacheMinutes < 1)
            {
                s.cacheMinutes = 360;
                _logger.Warn($"cacheMinutes задан некорректно. Установлено cacheMinutes={s.cacheMinutes}");
            }
            if (s.signingIntervalSeconds < 1 || s.signingIntervalSeconds > 60)
            {
                s.signingIntervalSeconds = 1;
                _logger.Warn($"signingIntervalSeconds задан некорректно. Установлено signingIntervalSeconds={s.signingIntervalSeconds}");
            }
        }
    }
}
