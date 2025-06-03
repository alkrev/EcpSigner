using Ecp.Web;
using EcpSigner.Infrastructure.WebClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
