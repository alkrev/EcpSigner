using Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal
{
    public class Main
    {
        Client wc;
        public Main(Client wc)
        {
            this.wc = wc;
        }
        /**
         * Выполняем вход
         */
        public async Task<loginReply> Login(string login, string password)
        {
            string url = $"?c=main&m=index&method=Logon&login={login}";
            string referer = "?c=portal&m=udp";
            var parameters = new Dictionary<string, string>() {
                { "login", login },
                { "psw", password },
                { "swUserRegion", "" },
                { "swUserDBType", "" },
            };
            loginReply data = await wc.PostJson<loginReply>(url, parameters, referer);
            return data;
        }
    }
    public class loginReply
    {
        public string Error_Msg = "";
        public bool success = false;
    }
}
