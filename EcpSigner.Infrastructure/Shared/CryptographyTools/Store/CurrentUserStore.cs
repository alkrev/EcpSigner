using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Adapters;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CryptographyTools.Store
{
    public class CurrentUserStore: ICurrentUserStore
    {
        private readonly CAPICOM.Store oStore;
        public CurrentUserStore()
        {
            oStore = new CAPICOM.Store();
        }
        /// <summary>
        /// Возвращает список валидных сертификатов пользователя
        /// </summary>
        public Dictionary<string, ICertificate> GetUserCertificates()
        {
            Dictionary<string, ICertificate> certs = new Dictionary<string, ICertificate>();
            try
            {
                oStore.Open(CAPICOM.CAPICOM_STORE_LOCATION.CAPICOM_CURRENT_USER_STORE);
                foreach (CAPICOM.ICertificate oCert in oStore.Certificates)
                {
                    certs[oCert.Thumbprint] = (ICertificate)new CertificateAdapter(oCert);
                }
                return certs;
            }
            catch (Exception ex)
            {
                throw new Exception("GetUserCertificates: " + ex.Message ?? "ошибка");
            }
        }
        /// <summary>
        /// Получение сертификата по отпечатку
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <returns>Возвращает сертификат или null если сертификат не найден</returns>
        public ICertificate GetCertificateByThumbprint(string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(thumbprint))
                throw new ArgumentException("отпечаток не может быть пустым");
            try
            {
                oStore.Open(CAPICOM.CAPICOM_STORE_LOCATION.CAPICOM_CURRENT_USER_STORE);
                CAPICOM.ICertificates2 certificates = (CAPICOM.ICertificates2)oStore.Certificates;
                certificates = certificates.Find(CAPICOM.CAPICOM_CERTIFICATE_FIND_TYPE.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, thumbprint, true);

                return certificates.Count > 0 ? (ICertificate)certificates[0] : null;
            }
            catch (Exception ex)
            {
                throw new Exception("GetCertificateByThumbprint: " + ex.Message ?? "ошибка");
            }
        }
        /// <summary>
        /// Усвобождаем неуправляемые ресурсы
        /// </summary>
        ~CurrentUserStore()
        {
            if (oStore != null)
            {
                Marshal.ReleaseComObject(oStore);
            }
        }
    }
}
