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
    }
}
