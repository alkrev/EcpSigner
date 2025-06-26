using Ecp.Web;

namespace EcpSigner.Infrastructure.Factories
{
    public interface IWebClientFactory
    {
        IClient Create(string url, string userAgent);
    }
}
