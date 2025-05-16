using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTools;

namespace EcpSigner.Infrastructure.WebClients
{
    public class WebClient : IWebClient
    {
        private IClient _wc;
        public WebClient(IClient wc)
        {
            _wc = wc;
        }
        public async Task<T> Post<T>(string url, Dictionary<string, string> parameters, string referer)
        {
            try
            {
                return await _wc.PostJson<T>(url, parameters, referer);
            }
            catch { throw; }
        }
    }
}
