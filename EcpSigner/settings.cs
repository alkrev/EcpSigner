using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EcpSigner
{
    public class Settings
    {
        public string login { get; set; }
        public string password { get; set; }
        public string url { get; set; }
        public int pauseMinutes { get; set; }
        public int cacheMinutes { get; set; }
        public int signingIntervalSeconds { get; set; }
        public List<string> ignoreDocTypes { get; set; }
        public Dictionary<string, byte> ignoreDocTypesDict { get; set; }
        public static Settings Read(string filename)
        {
            string str = File.ReadAllText(filename);
            Settings s = JsonConvert.DeserializeObject<Settings>(str);
            return s;
        }
    }
}
