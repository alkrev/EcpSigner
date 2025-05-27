using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Domain.Models;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Application.Decorators
{
    public class SignDocumentsLoopDecoratorTests
    {
        private readonly Mock<ISignDocumentsLoop> _innerMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly SignDocumentsLoopDecorator _decorator;

        public SignDocumentsLoopDecoratorTests()
        {
            _innerMock = new Mock<ISignDocumentsLoop>();
            _cacheMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger>();

            _decorator = new SignDocumentsLoopDecorator(
                _innerMock.Object,
                _cacheMock.Object,
                _loggerMock.Object,
                Mock.Of<IDelayProvider>()
            );
        }

        [Fact]
        public async Task RunAsync_Should_Call_Inner_RunAsync_And_Return_Result()
        {
            // Arrange
            var docs = new List<Document> { new Document() };
            var certs = new List<(EcpCertificate, ICertificate)>();
            var expectedResult = (signedCount: 2, docsToCache: new List<string> { "doc1", "doc2" });

            _innerMock
                .Setup(x => x.RunAsync(docs, certs, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _decorator.RunAsync(docs, certs, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _innerMock.Verify(x => x.RunAsync(docs, certs, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Cache_DocsToCache()
        {
            // Arrange
            var expectedDocsToCache = new List<string> { "doc1", "doc2" };
            _innerMock
                .Setup(x => x.RunAsync(It.IsAny<List<Document>>(), It.IsAny<List<(EcpCertificate, ICertificate)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((2, expectedDocsToCache));

            // Act
            await _decorator.RunAsync(new List<Document>(), new List<(EcpCertificate, ICertificate)>(), CancellationToken.None);

            // Assert
            _cacheMock.Verify(c => c.SetRange(expectedDocsToCache), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Log_Info_Messages()
        {
            // Arrange
            var docsToCache = new List<string> { "doc1" };
            _innerMock
                .Setup(x => x.RunAsync(It.IsAny<List<Document>>(), It.IsAny<List<(EcpCertificate, ICertificate)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((1, docsToCache));

            _cacheMock.Setup(c => c.Count()).Returns(1);

            // Act
            await _decorator.RunAsync(new List<Document>(), new List<(EcpCertificate, ICertificate)>(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains("подписываем документы"))), Times.Once);
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains("подписано документов [1]"))), Times.Once);
        }
    }
}
