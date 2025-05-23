using EcpSigner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Application.Filters
{
    public static class DocumentFilters
    {
        public static bool NeedsSigning(Document doc) => doc.SignStatus == "2" && string.IsNullOrEmpty(doc.Error);
        public static bool WithError(Document doc) => doc.SignStatus == "2" && !string.IsNullOrEmpty(doc.Error);
        public static bool IgnoredDocument(Document doc, Dictionary<string, byte> ignoreDocTypesDict) => ignoreDocTypesDict.ContainsKey(doc.Type);
    }
}
