using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EcpSigner.Application.Jobs;
using EcpSigner.Domain.Interfaces;
using EcpSigner.Application.Interfaces;
using EcpSigner.Domain.Models;
using EcpSigner.Domain.Exceptions;

namespace EcpSigner.Application.Jobs
{
    public class SignDocumentsLoopTests
    {
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IConfigurationProvider> _configMock = new();
        private readonly Mock<ISignDocumentWorflow> _workflowMock = new();
        private readonly Mock<IDelayProvider> _delayProviderMock = new();
        private readonly SignDocumentsLoop _loop;

        public SignDocumentsLoopTests()
        {
            _configMock.Setup(c => c.Get()).Returns(new AppSettings { signingIntervalSeconds = 1 });
            _loop = new SignDocumentsLoop(_loggerMock.Object, _configMock.Object, _workflowMock.Object, _delayProviderMock.Object);
        }

        [Fact]
        public async Task RunAsync_ShouldSignAllDocuments_WhenNoErrors()
        {
            // Arrange
            var docs = new List<Document>
        {
            new Document { ID = "1", Name = "Doc1", Num = "001", VersionNumber = 1 },
            new Document { ID = "2", Name = "Doc2", Num = "002", VersionNumber = 2 }
        };
            var certs = new List<(EcpCertificate, ICertificate)>();

            // Act
            var result = await _loop.RunAsync(docs, certs, CancellationToken.None);

            // Assert
            result.signedCount.Should().Be(2);
            result.docsToCache.Should().BeEmpty();
            _workflowMock.Verify(w => w.RunAsync(It.IsAny<Document>(), certs, It.IsAny<CancellationToken>()), Times.Exactly(2));
            _delayProviderMock.Verify(d => d.DelayAsync(TimeSpan.FromSeconds(1), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task RunAsync_ShouldSkipDocumentOnDocumentSigningException()
        {
            // Arrange
            var doc = new Document { ID = "1", Name = "Doc1", Num = "001", VersionNumber = 1 };
            var docs = new List<Document> { doc };
            var certs = new List<(EcpCertificate, ICertificate)>();

            _workflowMock
                .Setup(w => w.RunAsync(doc, certs, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DocumentSigningException("Signing failed"));

            // Act
            var result = await _loop.RunAsync(docs, certs, CancellationToken.None);

            // Assert
            result.signedCount.Should().Be(0);
            result.docsToCache.Should().ContainSingle().Which.Should().Be("1");
            _loggerMock.Verify(l => l.Warn(It.Is<string>(s => s.Contains("Signing failed"))), Times.Once);
        }

        [Fact]
        public async Task RunAsync_ShouldThrowOnIsNotLoggedInException()
        {
            // Arrange
            var doc = new Document { ID = "1", Name = "Doc1", Num = "001", VersionNumber = 1 };
            var docs = new List<Document> { doc };
            var certs = new List<(EcpCertificate, ICertificate)>();

            _workflowMock
                .Setup(w => w.RunAsync(doc, certs, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IsNotLoggedInException("пароль неверный"));

            // Act & Assert
            await Assert.ThrowsAsync<IsNotLoggedInException>(() => _loop.RunAsync(docs, certs, CancellationToken.None));
        }

        [Fact]
        public async Task RunAsync_ShouldThrowOnCancellation()
        {
            // Arrange
            var doc = new Document { ID = "1", Name = "Doc1", Num = "001", VersionNumber = 1 };
            var docs = new List<Document> { doc };
            var certs = new List<(EcpCertificate, ICertificate)>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<StopWorkException>(() => _loop.RunAsync(docs, certs, cts.Token));
        }

        [Fact]
        public async Task RunAsync_ShouldStopOnUnexpectedException()
        {
            // Arrange
            var doc1 = new Document { ID = "1", Name = "Doc1", Num = "001", VersionNumber = 1 };
            var doc2 = new Document { ID = "2", Name = "Doc2", Num = "002", VersionNumber = 2 };
            var docs = new List<Document> { doc1, doc2 };
            var certs = new List<(EcpCertificate, ICertificate)>();

            _workflowMock
                .Setup(w => w.RunAsync(doc1, certs, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _loop.RunAsync(docs, certs, CancellationToken.None);

            // Assert
            result.signedCount.Should().Be(0);
            result.docsToCache.Should().BeEmpty();
            _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("Unexpected error"))), Times.Once);
        }
        [Fact]
        public async Task RunAsync_ShouldThrowStopWorkException_WhenThrownFromWorkflow()
        {
            // Arrange
            var doc = new Document { ID = "1", Name = "Doc1", Num = "001", VersionNumber = 1 };
            var docs = new List<Document> { doc };
            var certs = new List<(EcpCertificate, ICertificate)>();

            _workflowMock
                .Setup(w => w.RunAsync(doc, certs, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new StopWorkException());

            // Act & Assert
            await Assert.ThrowsAsync<StopWorkException>(() => _loop.RunAsync(docs, certs, CancellationToken.None));
        }
    }
}
