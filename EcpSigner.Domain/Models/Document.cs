using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Models
{
    public class Document
    {
        ///<summary>Наименование документа</summary>
        public string Name = "";
        ///<summary>Номер документа</summary>
        public string Num = "";
        ///<summary>ID документа</summary>
        public string ID = "";
        ///<summary>Тип документа</summary>
        public string Type = "";
        ///<summary>ID версии документа</summary>
        public string VersionID = "";
        ///<summary>Ошибка</summary>
        public string Error = "";
        ///<summary>Статус подписания (IsSigned == "2" - требуется подпись МО)</summary>
        public string SignStatus = "";
        ///<summary>Номер версии документа</summary>
        public int VersionNumber;
        ///<summary>Требуется подпись и нет ошибок</summary>
    }
}
