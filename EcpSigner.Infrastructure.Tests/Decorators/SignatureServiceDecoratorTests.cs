using System;
using System.Collections.Generic;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Infrastructure.Decorators;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcpSigner.Infrastructure.Decorators
{
    public class SignatureServiceDecoratorTests
    {
        private readonly Mock<ISignatureService> _innerMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly SignatureServiceDecorator _decorator;

        public SignatureServiceDecoratorTests()
        {
            _innerMock = new Mock<ISignatureService>();
            _loggerMock = new Mock<ILogger>();
            _decorator = new SignatureServiceDecorator(_innerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetUserCertificates_ShouldLogAndReturnCertificates()
        {
            // Arrange
            var expectedCerts = new Dictionary<string, ICertificate>
            {
                { "cert1", Mock.Of<ICertificate>() },
                { "cert2", Mock.Of<ICertificate>() }
            };
            _innerMock.Setup(s => s.GetUserCertificates()).Returns(expectedCerts);

            // Act
            var result = _decorator.GetUserCertificates();

            // Assert
            result.Should().BeEquivalentTo(expectedCerts);
            _loggerMock.Verify(l => l.Debug("получаем список сертификатов пользователя"), Times.Once);
            _loggerMock.Verify(l => l.Debug(It.Is<string>(msg => msg.Contains("получено сертификатов пользователя"))), Times.Once);
        }

        [Fact]
        public void Sign_ShouldCallInnerSignAndLog()
        {
            // Arrange
            var cert = Mock.Of<ICertificate>();
            var docBase64 = "documentBase64";
            var document = "doc1";
            var expectedSignature = "signedContent";

            _innerMock.Setup(s => s.Sign(cert, docBase64, document)).Returns(expectedSignature);

            // Act
            var result = _decorator.Sign(cert, docBase64, document);

            // Assert
            result.Should().Be(expectedSignature);
            _innerMock.Verify(s => s.Sign(cert, docBase64, document), Times.Once);
            _loggerMock.Verify(l => l.Debug($"подпись {document} создана"), Times.Once);
        }
    }
}
