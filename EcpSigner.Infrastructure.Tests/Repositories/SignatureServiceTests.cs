using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using EcpSigner.Infrastructure.Repositories;
using EcpSigner.Infrastructure.Adapters;
using EcpSigner.Domain.Interfaces;
using CryptographyTools.Signing;
using CryptographyTools.Store;

namespace EcpSigner.Infrastructure.Repositories
{
    public class SignatureServiceTests
    {
        private readonly Mock<ISigning> _signingMock;
        private readonly Mock<ICurrentUserStore> _storeMock;
        private readonly SignatureService _service;

        public SignatureServiceTests()
        {
            _signingMock = new Mock<ISigning>();
            _storeMock = new Mock<ICurrentUserStore>();
            _service = new SignatureService(_signingMock.Object, _storeMock.Object);
        }

        [Fact]
        public void GetUserCertificates_ShouldReturnCertificates()
        {
            var comCert = new Mock<CAPICOM.ICertificate>().Object;
            var certAdapter = new CertificateAdapter(comCert);

            _storeMock.Setup(s => s.GetUserCertificates())
                      .Returns(new Dictionary<string, ICertificate>
                      {
                  { "cert1", certAdapter }
                      });

            var result = _service.GetUserCertificates();

            result.Should().ContainKey("cert1");
            result["cert1"].Should().BeOfType<CertificateAdapter>();
            ((CertificateAdapter)result["cert1"]).GetCertificate.Should().BeSameAs(comCert);
        }

        [Fact]
        public void Sign_WithCertificateAdapter_ShouldCallSignAndReturnResult()
        {
            // Arrange
            var comCertMock = new Mock<CAPICOM.ICertificate>().Object;
            var certificate = new CertificateAdapter(comCertMock);

            var base64 = "testBase64";
            var docName = "testDocument";
            var expectedSigned = "signedResult";

            _signingMock.Setup(s => s.Sign(comCertMock, base64))
                        .Returns(expectedSigned);

            // Act
            var result = _service.Sign(certificate, base64, docName);

            // Assert
            result.Should().Be(expectedSigned);
            _signingMock.Verify(s => s.Sign(comCertMock, base64), Times.Once);
        }

        [Fact]
        public void Sign_WithInvalidCertificate_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidCertificate = new Mock<ICertificate>().Object;

            // Act
            Action act = () => _service.Sign(invalidCertificate, "data", "doc");

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("неверный тип сертификата");
        }
    }
}
