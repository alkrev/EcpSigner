using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheTools
{
    public class Cache
    {
        private Dictionary<string, DateTime> cache;
        private int minutes;
        public Cache(int minutes)
        {
            this.minutes = minutes;
            cache = new Dictionary<string, DateTime>();
        }
        public void SetRange(List<string> numbers)
        {
            DateTime now = DateTime.UtcNow.AddMinutes(minutes);
            foreach (string num in numbers)
            {
                cache[num] = now;
            }
        }
        public bool Contains(string number)
        {
            if (cache.TryGetValue(number, out DateTime expirationTime))
            {
                if (expirationTime < DateTime.UtcNow)
                {
                    cache.Remove(number);
                    return false;
                }
                return true;
            }
            return false;
        }
        public int Count()
        {
            return cache.Count;
        }
        public void RemoveExpired()
        {
            DateTime now = DateTime.UtcNow;
            var expiredKeys = cache.Where(entry => entry.Value < now).Select(entry => entry.Key).ToList();
            foreach (string key in expiredKeys)
            {
                cache.Remove(key);
            }
        }
    }
}
