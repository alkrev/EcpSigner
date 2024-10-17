using Web;
using Portal;
using CacheTools;

namespace EcpSigner
{
    public class Portal
    {
        public EMD emd;
        public Main main;
        public bool isLoggedOn;
        public Cache cache;

        public Portal(Client wc, Cache c)
        {
            emd = new EMD(wc);
            main = new Main(wc);
            isLoggedOn = false;
            cache = c;
        }
    }
}
