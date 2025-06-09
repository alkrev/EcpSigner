using Ecp.Web;
using EcpSigner.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcpSigner.Infrastructure.WebClients
{
    public class WebClient : IClient
    {
        private IClient _wc;
        public WebClient(IClient wc)
        {
            _wc = wc;
        }
        public async Task<T> PostJson<T>(string url, Dictionary<string, string> parameters, string referer)
        {
            try
            {
                return await _wc.PostJson<T>(url, parameters, referer);
            }
            catch (Exception ex)
            {
                throw new ContinueExceptionWithError(ex.Message);
            }
        }
    }
}
