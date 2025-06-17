using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Adapters;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CryptographyTools.Store
{
    public class CurrentUserStore : ICurrentUserStore
    {
        private readonly CAPICOM.IStore3 _store;
        public CurrentUserStore(CAPICOM.IStore3 store)
        {
            _store = store;
        }
        /// <summary>
        /// Возвращает список валидных сертификатов пользователя
        /// </summary>
        public Dictionary<string, ICertificate> GetUserCertificates()
        {
            Dictionary<string, ICertificate> certs = new Dictionary<string, ICertificate>();
            try
            {
                _store.Open(CAPICOM.CAPICOM_STORE_LOCATION.CAPICOM_CURRENT_USER_STORE);
                foreach (CAPICOM.ICertificate oCert in _store.Certificates)
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
        /// Усвобождаем неуправляемые ресурсы
        /// </summary>
        ~CurrentUserStore()
        {
            if (_store != null && Marshal.IsComObject(_store))
            {
                Marshal.ReleaseComObject(_store);
            }
        }
    }
}
