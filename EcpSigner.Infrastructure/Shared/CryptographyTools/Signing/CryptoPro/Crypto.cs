using CAdESCOM;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CryptographyTools.Signing.CryptoPro
{
    public class Crypto: ISigning
    {
        private readonly CPSigner cPSigner;
        private readonly CadesSignedData cadesSignedData;
        public Crypto()
        {
            cPSigner = new CPSigner();
            cadesSignedData = new CadesSignedData();
        }
        /// <summary>
        /// Вычисляем подпись
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="docBase64"></param>
        /// <returns>Вычисленная подпись. Возвращает Exception.</returns>
        public string Sign(CAPICOM.ICertificate certificate, string docBase64)
        {
            try
            {
                cPSigner.Certificate = certificate;
                cPSigner.Options = CAPICOM.CAPICOM_CERTIFICATE_INCLUDE_OPTION.CAPICOM_CERTIFICATE_INCLUDE_WHOLE_CHAIN;
                cadesSignedData.ContentEncoding = CADESCOM_CONTENT_ENCODING_TYPE.CADESCOM_BASE64_TO_BINARY;
                cadesSignedData.Content = docBase64;
                string signatureBase64 = cadesSignedData.SignCades(cPSigner, CADESCOM_CADES_TYPE.CADESCOM_CADES_BES, true, CAPICOM_ENCODING_TYPE.CAPICOM_ENCODE_BASE64);
                return signatureBase64;
            }
            catch (Exception ex)
            {
                throw new Exception("Sign: " + ex.Message ?? "ошибка");
            }
        }
        /// <summary>
        /// Усвобождаем неуправляемые ресурсы
        /// </summary>
        ~Crypto()
        {
            if (cPSigner != null)
            {
                Marshal.ReleaseComObject(cPSigner);
            }
            if (cadesSignedData != null)
            {
                Marshal.ReleaseComObject(cadesSignedData);
            }
        }
    }
}
