using Ecp.Web;
using EcpSigner.Infrastructure.WebClients;

namespace EcpSigner.Infrastructure.Factories
{
    public class WebClientFactory: IWebClientFactory
    {
        public IClient Create(string url)
        {
            return new WebClient(new Client(url));
        }
    }
}
