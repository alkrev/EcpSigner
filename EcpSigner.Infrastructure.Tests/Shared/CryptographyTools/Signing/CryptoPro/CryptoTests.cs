using Xunit;
using Moq;
using FluentAssertions;
using CryptographyTools.Signing.CryptoPro;
using CAPICOM;
using CAdESCOM;
using System;

namespace EcpSigner.Infrastructure.Shared.CryptographyTools.Signing.CryptoPro
{
    public class CryptoTests
    {
        [Fact]
        public void Sign_ShouldReturnBase64Signature_WhenSigningIsSuccessful()
        {
            // Arrange
            var certificateMock = new Mock<ICertificate>();
            var signerMock = new Mock<ICPSigner6>();
            var signedDataMock = new Mock<ICPSignedData5>();

            var expectedSignature = "signed-data";

            // Настройка моков
            signerMock.SetupProperty(x => x.Certificate);
            signerMock.SetupProperty(x => x.Options);

            signedDataMock.SetupProperty(x => x.ContentEncoding);
            signedDataMock.SetupProperty(x => x.Content);
            signedDataMock
                .Setup(x => x.SignCades(signerMock.Object, CADESCOM_CADES_TYPE.CADESCOM_CADES_BES, true, CAdESCOM.CAPICOM_ENCODING_TYPE.CAPICOM_ENCODE_BASE64))
                .Returns(expectedSignature);

            var crypto = new Crypto(signerMock.Object, signedDataMock.Object);

            // Act
            var result = crypto.Sign(certificateMock.Object, "docBase64");

            // Assert
            result.Should().Be(expectedSignature);

            signerMock.Object.Certificate.Should().BeSameAs(certificateMock.Object);
            signerMock.Object.Options.Should().Be(CAPICOM_CERTIFICATE_INCLUDE_OPTION.CAPICOM_CERTIFICATE_INCLUDE_WHOLE_CHAIN);
            signedDataMock.Object.ContentEncoding.Should().Be(CADESCOM_CONTENT_ENCODING_TYPE.CADESCOM_BASE64_TO_BINARY);
            signedDataMock.Object.Content.Should().Be("docBase64");
        }

        [Fact]
        public void Sign_ShouldThrowExceptionWithPrefix_WhenSignCadesFails()
        {
            // Arrange
            var certificateMock = new Mock<ICertificate>();
            var signerMock = new Mock<ICPSigner6>();
            var signedDataMock = new Mock<ICPSignedData5>();

            var innerException = new InvalidOperationException("test-error");

            signedDataMock
                .Setup(x => x.SignCades(It.IsAny<ICPSigner6>(), It.IsAny<CADESCOM_CADES_TYPE>(), true, It.IsAny<CAdESCOM.CAPICOM_ENCODING_TYPE>()))
                .Throws(innerException);

            var crypto = new Crypto(signerMock.Object, signedDataMock.Object);

            // Act
            Action act = () => crypto.Sign(certificateMock.Object, "docBase64");

            // Assert
            act.Should().Throw<Exception>()
               .WithMessage("Sign: test-error");
        }
    }
}
