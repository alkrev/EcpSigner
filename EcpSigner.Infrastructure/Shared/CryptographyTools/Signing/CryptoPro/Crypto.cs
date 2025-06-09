using CAdESCOM;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CryptographyTools.Signing.CryptoPro
{
    public class Crypto: ISigning
    {
        private readonly ICPSigner6 _signer;
        private readonly ICPSignedData5 _signedData;
        public Crypto(ICPSigner6 signer, ICPSignedData5 signedData)
        {
            _signer = signer;
            _signedData = signedData;
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
                _signer.Certificate = certificate;
                _signer.Options = CAPICOM.CAPICOM_CERTIFICATE_INCLUDE_OPTION.CAPICOM_CERTIFICATE_INCLUDE_WHOLE_CHAIN;
                _signedData.ContentEncoding = CADESCOM_CONTENT_ENCODING_TYPE.CADESCOM_BASE64_TO_BINARY;
                _signedData.Content = docBase64;
                string signatureBase64 = _signedData.SignCades(_signer, CADESCOM_CADES_TYPE.CADESCOM_CADES_BES, true, CAPICOM_ENCODING_TYPE.CAPICOM_ENCODE_BASE64);
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
            if (_signer != null)
            {
                Marshal.ReleaseComObject(_signer);
            }
            if (_signedData != null)
            {
                Marshal.ReleaseComObject(_signedData);
            }
        }
    }
}
