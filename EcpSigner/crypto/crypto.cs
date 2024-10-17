using CAdESCOM;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cryptography
{
    static public class Crypto
    {
        /**
         * Получаем список валидных сертификатов пользователя
        */
        static public Dictionary<string, CAPICOM.ICertificate> GetUserCertificates()
        {
            CAPICOM.Store oStore = null;
            try
            {
                oStore = new CAPICOM.Store();
                oStore.Open(CAPICOM.CAPICOM_STORE_LOCATION.CAPICOM_CURRENT_USER_STORE);
                Dictionary<string, CAPICOM.ICertificate> certs = new Dictionary<string, CAPICOM.ICertificate>();
                foreach (CAPICOM.ICertificate oCert in oStore.Certificates)
                {
                    certs[oCert.Thumbprint] = oCert;
                }
                return certs;
            }
            finally
            {
                if (oStore != null)
                {
                    Marshal.ReleaseComObject(oStore);
                }
            }
        }
        /**
         * Вычисляем подпись
        */
        static public string Sign(CAPICOM.ICertificate certificate, string docBase64)
        {
            
            CPSigner cPSigner = null;
            CadesSignedData cadesSignedData = null;
            try
            {
                cPSigner = new CPSigner();
                cadesSignedData = new CadesSignedData();
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
            finally
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
}
