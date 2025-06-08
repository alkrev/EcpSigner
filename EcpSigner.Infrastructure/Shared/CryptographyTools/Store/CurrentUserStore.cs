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
                    certs[oCert.Thumbprint] = (ICertificate) new CertificateAdapter(oCert);
                }
                return certs;
            }
            catch (Exception ex)
            {
                throw new Exception("GetUserCertificates: " + ex.Message ?? "ошибка");
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
