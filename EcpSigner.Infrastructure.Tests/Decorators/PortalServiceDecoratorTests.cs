using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using EcpSigner.Infrastructure.Decorators;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcpSigner.Infrastructure.Decorators
{
    public class PortalServiceDecoratorTests
    {
        private readonly Mock<IPortalService> _innerMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly PortalServiceDecorator _decorator;

        public PortalServiceDecoratorTests()
        {
            _innerMock = new Mock<IPortalService>();
            _loggerMock = new Mock<ILogger>();
            _decorator = new PortalServiceDecorator(_innerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Login_Should_Log_Messages_And_Call_Inner()
        {
            // Arrange
            string login = "user";
            string password = "pass";

            // Act
            await _decorator.Login(login, password);

            // Assert
            _loggerMock.Verify(x => x.Info("выполняем вход"), Times.Once);
            _innerMock.Verify(x => x.Login(login, password), Times.Once);
            _loggerMock.Verify(x => x.Info("вход выполнен"), Times.Once);
        }

        [Fact]
        public async Task SearchDocuments_Should_Log_Before_And_After_And_Return_Result()
        {
            // Arrange
            string startDate = "2024-01-01";
            string endDate = "2024-01-31";
            var token = CancellationToken.None;
            var documents = new List<Document> { new Document(), new Document() };

            _innerMock.Setup(x => x.SearchDocuments(startDate, endDate, token))
                      .ReturnsAsync(documents);

            // Act
            var result = await _decorator.SearchDocuments(startDate, endDate, token);

            // Assert
            _loggerMock.Verify(x => x.Info($"получаем список документов {startDate}-{endDate}"), Times.Once);
            _loggerMock.Verify(x => x.Info(It.Is<string>(msg => msg.StartsWith("получено документов 2 за "))), Times.Once);
            result.Should().BeSameAs(documents);
        }

        [Fact]
        public async Task CheckBeforeSign_Should_Delegate_And_Log()
        {
            // Arrange
            var doc = new Document();
            var cert = new EcpCertificate();
            string docName = "doc1";

            // Act
            await _decorator.CheckBeforeSign(doc, cert, docName);

            // Assert
            _innerMock.Verify(x => x.CheckBeforeSign(doc, cert, docName), Times.Once);
            _loggerMock.Verify(x => x.Debug($"проверка перед подписанием документа {docName} прошла успешно"), Times.Once);
        }

        [Fact]
        public async Task GetSignData_Should_Delegate_And_Log_And_Return_Result()
        {
            // Arrange
            var doc = new Document();
            var cert = new EcpCertificate();
            string docName = "doc2";
            var expectedResult = ("docBase64", "hashBase64");

            _innerMock.Setup(x => x.GetSignData(doc, cert, docName))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _decorator.GetSignData(doc, cert, docName);

            // Assert
            _innerMock.Verify(x => x.GetSignData(doc, cert, docName), Times.Once);
            _loggerMock.Verify(x => x.Debug($"получение документа для подписания {docName} прошло успешно"), Times.Once);
            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task LoadEcpCertificates_Should_Log_And_Return_Result()
        {
            // Arrange
            var certs = new List<EcpCertificate> { new EcpCertificate(), new EcpCertificate(), new EcpCertificate() };
            _innerMock.Setup(x => x.LoadEcpCertificates())
                      .ReturnsAsync(certs);

            // Act
            var result = await _decorator.LoadEcpCertificates();

            // Assert
            _loggerMock.Verify(x => x.Debug("загружаем список сертификатов ЕЦП"), Times.Once);
            _loggerMock.Verify(x => x.Debug(It.Is<string>(msg => msg.StartsWith("загружено сертификатов ЕЦП 3 за "))), Times.Once);
            result.Should().BeSameAs(certs);
        }

        [Fact]
        public async Task SaveSignature_Should_Delegate_And_Log()
        {
            // Arrange
            var doc = new Document();
            string hash = "hash";
            string signature = "signature";
            var cert = new EcpCertificate();
            string docName = "doc3";

            // Act
            await _decorator.SaveSignature(doc, hash, signature, cert, docName);

            // Assert
            _innerMock.Verify(x => x.SaveSignature(doc, hash, signature, cert, docName), Times.Once);
            _loggerMock.Verify(x => x.Debug($"подпись документа {docName} сохранена на сервере"), Times.Once);
        }
    }
}
