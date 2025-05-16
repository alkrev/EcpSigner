using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTools
{
    public interface ICache
    {
        void SetRange(List<string> numbers);
        bool Contains(string number);
        int Count();
        void RemoveExpired();
    }
}
