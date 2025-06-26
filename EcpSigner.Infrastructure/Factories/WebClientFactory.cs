using Ecp.Web;
using EcpSigner.Infrastructure.WebClients;

namespace EcpSigner.Infrastructure.Factories
{
    public class WebClientFactory : IWebClientFactory
    {
        public IClient Create(string url, string userAgent)
        {
            return new WebClient(new Client(url, userAgent));
        }
    }
}
