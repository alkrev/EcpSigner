using CAdESCOM;
using CAPICOM;
using CryptographyTools.Store;
using EcpSigner.Infrastructure.Adapters;
using FluentAssertions;
using Microsoft.VisualBasic;
using Moq;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace EcpSigner.Infrastructure.Shared.CryptographyTools.Store
{
    public class CurrentUserStoreTests
    {
        [Fact]
        public void GetUserCertificates_ShouldReturnDictionaryOfCertificates_WhenStoreContainsCertificates()
        {
            // Arrange
            var storeMock = new Mock<CAPICOM.IStore3>();
            var certMock1 = new Mock<CAPICOM.ICertificate>();
            var certMock2 = new Mock<CAPICOM.ICertificate>();
            var certificatesMock = new Mock<CAPICOM.ICertificates>();

            certMock1.Setup(c => c.Thumbprint).Returns("thumb1");
            certMock2.Setup(c => c.Thumbprint).Returns("thumb2");

            var certs = new CAPICOM.ICertificate[] { certMock1.Object, certMock2.Object };

            certificatesMock.Setup(x => x.Count).Returns(certs.Count);
            certificatesMock.Setup(x => x.GetEnumerator()).Returns(certs.GetEnumerator);

            storeMock.Setup(s => s.Open(It.IsAny<CAPICOM.CAPICOM_STORE_LOCATION>(), It.IsAny<string>(), It.IsAny<CAPICOM_STORE_OPEN_MODE>()));
            storeMock.Setup(s => s.Certificates).Returns(certificatesMock.Object);

            var store = new CurrentUserStore(storeMock.Object);

            // Act
            var result = store.GetUserCertificates();

            // Assert
            result.Should().ContainKey("thumb1");
            result.Should().ContainKey("thumb2");
            result["thumb1"].Should().BeOfType<CertificateAdapter>();
            result["thumb2"].Should().BeOfType<CertificateAdapter>();
        }

        [Fact]
        public void GetUserCertificates_ShouldThrowException_WhenStoreThrows()
        {
            // Arrange
            var storeMock = new Mock<CAPICOM.IStore3>();
            storeMock.Setup(s => s.Open(It.IsAny<CAPICOM.CAPICOM_STORE_LOCATION>(), It.IsAny<string>(), It.IsAny<CAPICOM_STORE_OPEN_MODE>()))
                     .Throws(new InvalidOperationException("Store open error"));

            var store = new CurrentUserStore(storeMock.Object);

            // Act
            Action act = () => store.GetUserCertificates();

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Store open error*");
        }
    }
}
