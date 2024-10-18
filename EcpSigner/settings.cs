using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace EcpSigner
{
    public class Settings
    {
        public string login { get; set; }
        public string password { get; set; }
        public string url { get; set; }
        public int threadCount { get; set; }
        public int cacheMinutes { get; set; }
        public List<string> ignoreDocTypes { get; set; }
        public Dictionary<string, byte> ignoreDocTypesDict { get; set; }
   
        public static Settings Read(string filename)
        {
            string str = File.ReadAllText(filename);
            Settings s = JsonConvert.DeserializeObject<Settings>(str);
            s.Init();
            return s;
        }
        private void Init()
        {
            ignoreDocTypesDict = ignoreDocTypes.ToDictionary(x => x, x => (byte)1);
        }
        public void CheckSettings(NLog.Logger logger)
        {
            if (string.IsNullOrEmpty(login))
            {
                throw new Exception("login пользователя не задан");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new Exception("password пользователя не задан");
            }
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("url сайта ЕЦП не задан");
            }
            if (threadCount<1)
            {
                threadCount = 10;
                logger.Warn($"threadCount задан некорректно. Установлено threadCount={threadCount}");
            }
            if (cacheMinutes<1)
            {
                cacheMinutes = 360;
                logger.Warn($"cacheMinutes задан некорректно. Установлено cacheMinutes={cacheMinutes}");
            }
        }
    }
}
