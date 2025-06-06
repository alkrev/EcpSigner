using EcpSigner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CachingTools
{
    public class Cache : ICache
    {
        private readonly Dictionary<string, DateTime> cache;
        private readonly int minutes;
        private readonly IDateTimeProvider dateTimeProvider;

        public Cache(int minutes, IDateTimeProvider dateTimeProvider)
        {
            this.minutes = minutes;
            this.dateTimeProvider = dateTimeProvider;
            cache = new Dictionary<string, DateTime>();
        }

        public void SetRange(List<string> numbers)
        {
            DateTime expirationTime = dateTimeProvider.Now.AddMinutes(minutes);
            foreach (string num in numbers)
            {
                cache[num] = expirationTime;
            }
        }

        public bool Contains(string number)
        {
            if (cache.TryGetValue(number, out DateTime expirationTime))
            {
                if (expirationTime < dateTimeProvider.Now)
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
            DateTime now = dateTimeProvider.Now;
            var expiredKeys = cache
                .Where(entry => entry.Value < now)
                .Select(entry => entry.Key)
                .ToList();

            foreach (string key in expiredKeys)
            {
                cache.Remove(key);
            }
        }
    }
}
