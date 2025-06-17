using System.Collections.Generic;

namespace CachingTools
{
    public interface ICache
    {
        void SetRange(List<string> numbers);
        bool Contains(string number);
        int Count();
        void RemoveExpired();
    }
}
