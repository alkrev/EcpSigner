using System;
using System.Collections.Generic;

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
            DateTime t = DateTime.UtcNow.AddMinutes(minutes);
            foreach (string num in numbers)
            {
                cache[num] = t;
            }
        }

        public bool Contains(string number)
        {
            if (cache.ContainsKey(number)) {
                if (cache[number] < DateTime.UtcNow)
                {
                    cache.Remove(number);
                    return false;
                }
                else
                {
                    return true;
                }
            } 
            else
            {
                return false;
            }
        }
        public int Count()
        {
            return cache.Count;
        }
    }
}
