using CryptographyTools.Store;
using FluentAssertions;

namespace EcpSigner.Infrastructure.Shared.CryptographyTools.Store
{
    public class CurrentUserStoreTests
    {
        private readonly CurrentUserStore _store;

        public CurrentUserStoreTests()
        {
            _store = new CurrentUserStore();
        }

        [Fact]
        public void GetUserCertificates_ShouldReturnNonEmptyCollection_WhenCertificatesExist()
        {
            // Act
            var certificates = _store.GetUserCertificates();

            // Assert
            certificates.Should().NotBeNull();
            certificates.Count.Should().BeGreaterThan(0, "должен быть хотя бы один установленный сертификат");

            foreach (var cert in certificates.Values)
            {
                cert.Subject.Should().NotBeNullOrEmpty();
                cert.ValidToDate.Should().BeAfter(DateTime.MinValue);
            }
        }
    }
}
