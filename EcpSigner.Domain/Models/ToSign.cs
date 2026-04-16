using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Models
{
    public class ToSign
    {
        ///<summary>Версия документа для подписания</summary>
        public string EMDVersion_id = "";
        ///<summary>Документ для подписания</summary>
        public string docBase64 = "";
        ///<summary>Хеш документа для подписания<</summary>
        public string hashBase64 = "";
    }
}
